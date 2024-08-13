using DNDocs.Application.Application;
using DNDocs.Application.Commands.Projects;
using DNDocs.Application.Services;
using DNDocs.Application.Shared;
using DNDocs.Domain.Entity.App;
using DNDocs.Domain.Enums;
using DNDocs.Domain.Repository;
using DNDocs.Domain.Service;
using DNDocs.Domain.UnitOfWork;
using DNDocs.Domain.Utils;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Application.CommandHandlers.Projects
{
    internal class CreateSingletonProjectHandler : CommandHandlerA<CreateSingletonProjectCommand, int>
    {
        private IProjectManager projectManager;
        private IAppUnitOfWork uow;
        private ICurrentUser user;
        private IProjectRepository projectRepository;
        private IBgJobQueue bgjobQueue;
        private ApiBackgroundWorker abw;

        public CreateSingletonProjectHandler(
            IProjectManager projectManager,
            ICurrentUser user,
            IAppUnitOfWork uow,
            IBgJobQueue bgjobQueue,
            ApiBackgroundWorker abw)
        {
            this.projectManager = projectManager;
            this.uow = uow;
            this.user = user;
            this.projectRepository = uow.ProjectRepository;
            this.bgjobQueue = bgjobQueue;
            this.abw = abw;
        }

        public override async Task<int> Handle(CreateSingletonProjectCommand c)
        {
            Validation.NotStringIsNullOrWhiteSpace(c.ProjectName, "Project name cannot be empty");
            Validation.NotStringIsNullOrWhiteSpace(c.UrlPrefix, "Url prefix cannot be empty");
            Validation.ThrowError(c.NugetPackages == null || c.NugetPackages.Count == 0, "Nuget packages are empty");
            // Validation.ThrowError(!string.IsNullOrWhiteSpace(c.DocfxTemplate) && !docfxManager.TemplateExists(c.DocfxTemplate), $"Template does not exists: '{c.DocfxTemplate}'");

            var invalidSameNames = await projectRepository.Query()
                .AnyAsync(t => t.ProjectType == ProjectType.Singleton && t.ProjectName == c.ProjectName);

            Validation.ThrowError(invalidSameNames, "Project name is not unique in the system. Use other name");

            var invalidSameUrlPrefix = await projectRepository.Query()
                .AnyAsync(t => t.ProjectType == ProjectType.Singleton && c.UrlPrefix == t.UrlPrefix);

            Project.ValidateUrlPrefix(c.UrlPrefix, nameof(c.UrlPrefix));
            Validation.ThrowError(invalidSameUrlPrefix, "urlprefix is not unique in the system");

            string gitDocsCommitHash = null;
            if (string.IsNullOrWhiteSpace(c.GitMdRepoUrl))
            {
                var errMsg = "Field '{0}' must be empty if there is no github md docs repository";
                Validation.ThrowError(!string.IsNullOrEmpty(c.GitMdBranchName), string.Format(errMsg, "GitMdBranchName"));
                Validation.ThrowError(!string.IsNullOrEmpty(c.GitMdRelativePathDocs), string.Format(errMsg, "GitMdRelativePathDocs"));
                Validation.ThrowError(!string.IsNullOrEmpty(c.GitMdRelativePathReadme), string.Format(errMsg, "GitMdRelativePathReadme"));
                Validation.ThrowError(!string.IsNullOrEmpty(c.GitDocsCommitHash), string.Format(errMsg, "GitDocsCommitHash"));
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(c.GitMdRepoUrl))
                    gitDocsCommitHash = projectManager.NewestMdDocsCommit(c.GitMdRepoUrl, c.GitMdRelativePathDocs, c.GitMdRelativePathReadme);

                await projectManager.ValidateGitRepository(c.GitMdRepoUrl, c.GitMdBranchName, c.GitMdRelativePathReadme, c.GitMdRelativePathDocs);

                Validation.NotStringIsNullOrWhiteSpace(gitDocsCommitHash);
            }

            var project = new Project(
                c.ProjectName,
                c.UrlPrefix,
                c.Description,
                c.GithubUrl,
                c.DocfxTemplate,
                ProjectState.NotActive,
                null,
                c.GitMdRepoUrl,
                c.GitMdBranchName,
                c.GitMdRelativePathDocs,
                c.GitMdRelativePathReadme,
                gitDocsCommitHash,
                c.PSAutoRebuild,
                ProjectType.Singleton,
                null,
                null);

            project.StateDetails = ProjectStateDetails.WaitingToBuild;

            project.AddUser(uow.GetSimpleRepository<User>().GetByIdChecked(user.UserIdAuthorized));
            var projectNugetPackages = await projectManager.ValidateAndCreateNugetPackages(c.NugetPackages, true);

            project.ProjectNugetPackages = projectNugetPackages;

            await uow.GetSimpleRepository<Project>().CreateAsync(project);
            await uow.SaveChangesAsync();

            abw.DoSystemWorkNow();

            return project.Id;
        }
    }
}

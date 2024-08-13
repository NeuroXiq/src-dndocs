using Microsoft.EntityFrameworkCore;
using DNDocs.Application.Commands.Projects;
using DNDocs.Application.Shared;
using DNDocs.Domain.Entity.App;
using DNDocs.Domain.Repository;
using DNDocs.Domain.Service;
using DNDocs.Domain.UnitOfWork;
using DNDocs.Domain.Utils;
using DNDocs.Domain.ValueTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Application.CommandHandlers.Projects
{
    class CreateProjectVersioningHandler : CommandHandlerA<CreateProjectVersioningCommand, int>
    {
        private IAppManager appManager;
        private ICurrentUser user;
        private IAppUnitOfWork uow;
        private IProjectManager projectManager;
        private IRepository<ProjectVersioning> versioningRepo;

        public CreateProjectVersioningHandler(
            ICurrentUser user,
            IAppUnitOfWork uow,
            IProjectManager projectManager,
            IAppManager appManager)
        {
            this.appManager = appManager;
            this.user = user;
            this.uow = uow;
            this.projectManager = projectManager;
            this.versioningRepo = uow.GetSimpleRepository<ProjectVersioning>();
        }

        public override async Task<int> Handle(CreateProjectVersioningCommand command)
        {
            Validation.NotStringIsNullOrWhiteSpace(command.ProjectName);
            Validation.NotStringIsNullOrWhiteSpace(command.GitDocsRepoUrl, "For versioning git repository must exists to provide tag names");
            Validation.FieldErrorP(
                await versioningRepo.Query().Where(t => t.ProjectName == command.ProjectName).AnyAsync(),
                command.ProjectName,
                "Versioning with this project name already exists");

            Validation.FieldErrorP(string.IsNullOrWhiteSpace(command.UrlPrefix), command.UrlPrefix, "Url prefix cannot be null or empty");
            
            Validation.FieldErrorP(
                await versioningRepo.Query().Where(t => t.UrlPrefix == command.UrlPrefix).AnyAsync(),
                command.UrlPrefix,
                "Url prefix already assigned to other versioning");
            Project.ValidateUrlPrefix(command.UrlPrefix, nameof(command.UrlPrefix));
            projectManager.ValidateGitRepository(command.GitDocsRepoUrl, command.GitDocsBranchName, command.GitHomepageRelativePath, command.GitDocsRelativePath);

            if (!string.IsNullOrWhiteSpace(command.GitDocsRepoUrl))
            {
                projectManager.ValidateGitRepository(
                    command.GitDocsRepoUrl,
                    command.GitDocsBranchName,
                    command.GitHomepageRelativePath,
                    command.GitDocsRelativePath);
            }

            var packages = command.NugetPackages.Select(p => new NugetPackageDto(p.IdentityId, null)).ToArray();

            var versioning = new ProjectVersioning(
                user.UserIdAuthorized,
                command.ProjectName,
                command.ProjectWebsiteUrl,
                command.UrlPrefix,
                command.GitDocsRepoUrl,
                command.GitDocsBranchName,
                command.GitDocsRelativePath,
                command.GitHomepageRelativePath,
                command.Autoupgrage);

            var nugetPackages = await projectManager.ValidateAndCreateNugetPackages(packages, true);
            versioning.NugetPackages.AddRange(nugetPackages);

            await uow.GetSimpleRepository<ProjectVersioning>().CreateAsync(versioning);

            return versioning.Id;
        }
    }
}
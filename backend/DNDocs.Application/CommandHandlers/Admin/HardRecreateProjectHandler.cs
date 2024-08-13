using DNDocs.Application.Commands.Admin;
using DNDocs.Application.Shared;
using DNDocs.Domain.Entity.App;
using DNDocs.Domain.Repository;
using DNDocs.Domain.Service;
using DNDocs.Domain.UnitOfWork;
using DNDocs.Domain.ValueTypes;
using DNDocs.Domain.ValueTypes.Project;

namespace DNDocs.Application.CommandHandlers.Admin
{
    internal class HardRecreateProjectHandler : CommandHandler<HardRecreateProjectCommand, int>
    {
        private IAppUnitOfWork uow;
        private IProjectManager projectManager;
        private IRepository<Project> projectRepo;

        public HardRecreateProjectHandler(
            IProjectManager projectManager,
            IAppUnitOfWork uow
            )
        {
            this.uow = uow;
            this.projectManager = projectManager;
            projectRepo = uow.GetSimpleRepository<Project>();
        }

        public override int Handle(HardRecreateProjectCommand command)
        {
            var project = projectRepo.Query(t => t.ProjectNugetPackages, t => t.RefUserProject).Where(t => t.Id == command.ProjectId).First();
            var nupks = project.ProjectNugetPackages.Select(t => new NugetPackageDto(t.IdentityId, t.IdentityVersion)).ToArray();
            var userid = project.RefUserProject.First().UserId;

            projectManager.DeleteProject(command.ProjectId);
            
            var createParams = new CreateProjectParams(
                project.ProjectName,
                project.GithubUrl,
                project.DocfxTemplate,
                project.Description,
                project.UrlPrefix,
                nupks,
                userid,
                project.GitMdRepoUrl,
                project.GitMdBranchName,
                project.GitMdRelativePathDocs,
                project.GitMdRelativePathReadme,
                null,
                project.PSAutorebuild,
                project.ProjectType,
                project.NugetOrgPackageName,
                project.NugetOrgPackageVersion);

            return projectManager.CreateSingletonProject(createParams).Result;
        }
    }
}

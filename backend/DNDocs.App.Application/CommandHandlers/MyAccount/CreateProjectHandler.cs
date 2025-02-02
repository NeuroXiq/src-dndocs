//using DNDocs.Application.Commands.MyAccount;
//using DNDocs.Application.Shared;
//using DNDocs.Docs.Api.Management;
//using DNDocs.Domain.Service;
//using DNDocs.Domain.Services;
//using DNDocs.Domain.UnitOfWork;
//using DNDocs.Domain.Utils;
//using DNDocs.Domain.ValueTypes;
//using DNDocs.Domain.ValueTypes.Project;

//namespace DNDocs.Application.CommandHandlers.MyAccount
//{
//    internal class CreateProjectHandler : CommandHandlerA<CreateProjectCommand, int>
//    {
//        private IAppUnitOfWork uow;
//        private IProjectManager projectManager;
//        private INugetRepositoryFacade nugetRepositoryFacade;

//        public CreateProjectHandler(
//            IProjectManager projectManager,
//            IAppUnitOfWork uow,
//            INugetRepositoryFacade nugetRepositoryFacade)
//        {
//            this.uow = uow;
//            this.projectManager = projectManager;
//            this.nugetRepositoryFacade = nugetRepositoryFacade;
//        }

//        public override async Task<int> Handle(CreateProjectCommand command)
//        {
//            var cpp = command.CreateProjectParams;
//            var packages = cpp.NugetPackages ?? new NugetPackageDto[0];
//            // List<NugetPackageDto> npkg = new List<NugetPackageDto>();

//            var createParams = new CreateProjectParams(
//                           cpp.ProjectName,
//                           cpp.GithubUrl,
//                           cpp.DocfxTemplate,
//                           cpp.Description,
//                           cpp.UrlPrefix,
//                           packages.ToArray(),
//                           command.UserId,
//                           cpp.GitMdRepoUrl,
//                           cpp.GitMdBranchName,
//                           cpp.GitMdRelativePathDocs,
//                           cpp.GitMdRelativePathReadme,
//                           null,
//                           cpp.PSAutorebuild,
//                           cpp.ProjectType,
//                           cpp.NugetPackageName,
//                           cpp.NugetPackageVersion);

//            return await projectManager.CreateSingletonProject(createParams);
//        }
//    }
//}

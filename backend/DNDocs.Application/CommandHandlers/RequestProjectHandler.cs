//using DNDocs.Application.Application;
//using DNDocs.Application.Commands;
//using DNDocs.Application.Commands.MyAccount;
//using DNDocs.Application.Shared;
//using DNDocs.Domain.Entity.App;
//using DNDocs.Domain.Service;
//using DNDocs.Domain.UnitOfWork;
//using DNDocs.Domain.Utils;
//using DNDocs.Domain.ValueTypes;
//using DNDocs.Domain.ValueTypes.Project;

//namespace DNDocs.Application.CommandHandlers
//{
//    internal class RequestProjectHandler : CommandHandlerA<RequestProjectCommand, int>
//    {
//        private ICurrentUser user;
//        private IAppUnitOfWork uow;
//        private IProjectManager projectManager;
//        private ApiBackgroundWorker abw;

//        public RequestProjectHandler(
//            IProjectManager projectManager,
//            IAppUnitOfWork uow,
//            ApiBackgroundWorker abw,
//            ICurrentUser user,
//            ApiBackgroundWorker apiBackgroundWorker)
//        {
//            this.user = user;
//            this.uow = uow;
//            this.projectManager = projectManager;
//            this.abw = abw;
//        }

//        public override async Task<int> Handle(RequestProjectCommand command)
//        {
//            Validation.ThrowFieldError(nameof(command.CreateProjectParams), "Field cannot be null", command.CreateProjectParams == null);

//            var cpp = command.CreateProjectParams;
//            var npkg = command.CreateProjectParams.NugetPackages?.Select(t => new NugetPackageDto(t.IdentityId, t.IdentityVersion)).ToArray() ?? new NugetPackageDto[0];
            
//            var npkgDto = (await projectManager
//                .ValidateAndCreateNugetPackages(npkg, true))
//                .Select(t => new NugetPackageDto(t.IdentityId, t.IdentityVersion))
//                .ToArray();

//            var createParams = new CreateProjectParams(
//                cpp.ProjectName,
//                cpp.GithubUrl,
//                cpp.DocfxTemplate,
//                cpp.Description,
//                cpp.UrlPrefix,
//                npkgDto,
//                user.UserIdAuthorized,
//                cpp.GitMdRepoUrl,
//                cpp.GitMdBranchName,
//                cpp.GitMdRelativePathDocs,
//                cpp.GitMdRelativePathReadme,
//                null,
//                cpp.PSAutorebuild,
//                ProjectType.Singleton,
//                null,
//                null);

//            projectManager.ValidateCreate(createParams, isRawUserInput: true);

//            var tq = new CreateProjectCommand(cpp, user.UserIdAuthorized);

//            // var bgjobid = abw.Push(tq, createByUserId: user.UserIdAuthorized);

//            //return bgjobid;
//            throw new NotSupportedException();
//        }
//    }
//}

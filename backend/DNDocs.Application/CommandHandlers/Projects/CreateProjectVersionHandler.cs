using DNDocs.Application.Application;
using DNDocs.Application.Commands.Projects;
using DNDocs.Application.Shared;
using DNDocs.Domain.Entity.App;
using DNDocs.Domain.Repository;
using DNDocs.Domain.Service;
using DNDocs.Domain.UnitOfWork;
using DNDocs.Domain.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Application.CommandHandlers.Projects
{
    class CreateProjectVersionHandler : CommandHandlerA<CreateProjectVersionCommand, int>
    {
        private IProjectManager projectManager;
        private ApiBackgroundWorker bgworker;
        private ICurrentUser user;
        private IAppManager appManager;
        private IRepository<ProjectVersioning> projectVersioningRepo;

        public CreateProjectVersionHandler(
            IAppUnitOfWork uow,
            IAppManager appManager,
            ICurrentUser user,
            ApiBackgroundWorker bgworker,
            IProjectManager projectManager)
        {
            this.projectManager = projectManager;
            this.bgworker = bgworker;
            this.user = user;
            this.appManager = appManager;
            this.projectVersioningRepo = uow.GetSimpleRepository<ProjectVersioning>();
        }

        public override async Task<int> Handle(CreateProjectVersionCommand command)
        {
            // 1. validate
            // await projectManager.ValidateCreateProjectVersion(
            //     command.ProjectVersioningId,
            //     command.GitTagName,
            //     command.NugetPackages?.ToArray() ?? new Domain.ValueTypes.DTO.NugetPackageDto[0]);
            // 
            // var bgjobCommand = new BgJobCreateProjectVersionCommand(command.ProjectVersioningId, command.GitTagName, command.NugetPackages);
            // var jobid = bgworker.Push(bgjobCommand, createByUserId: user.UserIdAuthorized);
            // return jobid;
            throw new NotSupportedException();
        }
    }
}

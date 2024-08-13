using Microsoft.AspNetCore.Mvc;
using DNDocs.Application.Application;
using DNDocs.Application.Commands.Projects;
using DNDocs.Application.Shared;
using DNDocs.Domain.Entity.App;
using DNDocs.Domain.Service;
using DNDocs.Domain.UnitOfWork;
using DNDocs.Domain.Utils;
using DNDocs.Domain.ValueTypes;
using DNDocs.Infrastructure.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Application.CommandHandlers.Projects
{
    internal class CreateProjectVersionByGitTagHandler : CommandHandlerA<CreateProjectVersionByGitTagCommand, int>
    {
        private IAppUnitOfWork uow;
        private ICurrentUser user;
        private ApiBackgroundWorker bg;
        private IProjectManager projectManager;
        private IScopeContext scopeContext;

        public CreateProjectVersionByGitTagHandler(
            ICurrentUser user,
            IScopeContext scopeContext,
            IProjectManager projectManager,
            IAppUnitOfWork uow,
            ApiBackgroundWorker bg)
        {
            this.uow = uow;
            this.user = user;
            this.bg = bg;
            this.projectManager = projectManager;
            this.scopeContext = scopeContext;
        }

        public override async Task<int> Handle(CreateProjectVersionByGitTagCommand command)
        {
            if (scopeContext.BgJobId.HasValue)
            {
                // now running as backgroud job, execute action
                // return await projectManager.CreateVersionProject(command.ProjectVersioningId, command.GitTagName);
            }

            // await projectManager.ValidateCreateProjectVersion(command.ProjectVersioningId, command.GitTagName, new List<NugetPackageDto>());

            // return bg.Push(command, user.UserIdAuthorized);

            throw new NotSupportedException();
        }
    }
}

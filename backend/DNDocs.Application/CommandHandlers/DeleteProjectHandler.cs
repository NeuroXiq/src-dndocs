using DNDocs.Application.Commands;
using DNDocs.Application.Shared;
using DNDocs.Domain.Service;
using DNDocs.Domain.UnitOfWork;
using DNDocs.Domain.Utils;

namespace DNDocs.Application.CommandHandlers
{
    internal class DeleteProjectHandler : CommandHandler<DeleteProjectCommand>
    {
        private IAppManager appManager;
        private IAppUnitOfWork appUow;
        private ICurrentUser user;
        private IProjectManager projectManager;

        public DeleteProjectHandler(IProjectManager projectManager)
        {
            this.projectManager = projectManager;
        }

        public override void Handle(DeleteProjectCommand command)
        {
            projectManager.DeleteProject(command.ProjectId);
        }
    }
}

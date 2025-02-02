using DNDocs.Application.Shared;
using DNDocs.Domain.Service;
using DNDocs.Domain.Utils;

namespace DNDocs.Application.Commands.Projects
{
    internal class AutoupgradeSingletonProjectHandler : CommandHandler<AutoupgradeSingletonProjectCommand>
    {
        private IProjectManager projectManager;
        private ICurrentUser user;

        public AutoupgradeSingletonProjectHandler(IProjectManager projectManager, ICurrentUser user)
        {
            this.projectManager = projectManager;
            this.user = user;
        }

        public override void Handle(AutoupgradeSingletonProjectCommand command)
        {
            projectManager.AutoupgradeSingleton(command.ProjectId);
        }
    }
}

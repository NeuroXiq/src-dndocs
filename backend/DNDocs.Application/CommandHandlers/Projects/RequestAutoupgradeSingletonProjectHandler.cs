using DNDocs.Application.Application;
using DNDocs.Application.Commands.Projects;
using DNDocs.Application.Shared;
using DNDocs.Domain.Service;
using DNDocs.Domain.UnitOfWork;
using DNDocs.Domain.Utils;

namespace DNDocs.Application.CommandHandlers.Projects
{
    internal class RequestAutoupgradeSingletonProjectHandler : CommandHandler<RequestAutoupgradeSingletonProjectCommand, int>
    {
        private ICurrentUser user;
        private ApiBackgroundWorker apiBackgroundWorker;
        private IAppUnitOfWork appuow;

        public RequestAutoupgradeSingletonProjectHandler(
            IProjectManager projectManager,
            ApiBackgroundWorker apiBackgroundWorker,
            ICurrentUser user,
            IAppUnitOfWork appuow)
        {
            this.user = user;
            this.apiBackgroundWorker = apiBackgroundWorker;
            this.appuow = appuow;
        }

        public override int Handle(RequestAutoupgradeSingletonProjectCommand command)
        {
            var project = appuow.ProjectRepository.GetByIdChecked(command.ProjectId);
            var bgjobCmd = new AutoupgradeSingletonProjectCommand(command.ProjectId);
            Validation.ThrowError(project.PVProjectVersioningId.HasValue, "Cannot autoupgrade versioning project");

            // return apiBackgroundWorker.Push(bgjobCmd, user.UserIdAuthorized);
            throw new NotSupportedException();
        }
    }
}

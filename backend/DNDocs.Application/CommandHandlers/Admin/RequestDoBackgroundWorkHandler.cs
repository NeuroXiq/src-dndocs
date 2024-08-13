using DNDocs.Application.Application;
using DNDocs.Application.Commands.Admin;
using DNDocs.Application.Shared;

namespace DNDocs.Application.CommandHandlers.Admin
{
    internal class RequestDoBackgroundWorkHandler : CommandHandler<RequestDoBackgroundWorkCommand>
    {
        private ApiBackgroundWorker apiBackgroundWorker;

        public RequestDoBackgroundWorkHandler(ApiBackgroundWorker apiBackgroundWorker)
        {
            this.apiBackgroundWorker = apiBackgroundWorker;
        }

        public override void Handle(RequestDoBackgroundWorkCommand command)
        {
            var cmd = new DoBackgroundWorkCommand
            {
                ForceAll = command.ForceAll,
                ForceQueuedItems = command.ForceQueuedItems,
                ForceCheckHttpStatusForProjects = command.ForceCheckHttpStatusForProjects,
            };

            apiBackgroundWorker.DoSystemWorkNow(cmd);
        }
    }
}

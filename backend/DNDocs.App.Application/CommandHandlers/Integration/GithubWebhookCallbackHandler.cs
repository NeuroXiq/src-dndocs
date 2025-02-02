using DNDocs.Application.Commands.Integration;
using DNDocs.Application.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Application.CommandHandlers.Integration
{
    internal class GithubWebhookCallbackHandler : CommandHandlerA<GithubWebhookCallbackCommand>
    {
        public override async Task Handle(GithubWebhookCallbackCommand command)
        {
            // todo for future github webhook integratino

            return;
        }
    }
}

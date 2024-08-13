using DNDocs.Application.Commands.Integration;
using DNDocs.Application.Shared;
using DNDocs.Domain.Entity.App;
using DNDocs.Domain.Utils;
using DNDocs.Job.Api.Client;
using DNDocs.Shared.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Application.CommandHandlers.Integration
{
    internal class DJobRegisterServiceHandler : CommandHandlerA<DJobRegisterServiceCommand>
    {
        private IDJobClientFactory djobClientFactory;
        private DNDocsSettings settings;

        public DJobRegisterServiceHandler(IDJobClientFactory djobClientFactory, IOptions<DNDocsSettings> settings)
        {
            this.djobClientFactory = djobClientFactory;
            this.settings = settings.Value;
        }

        public override async Task Handle(DJobRegisterServiceCommand command)
        {
            logger.LogInformation("registering djob service: {0} {1} {2}", command.InstanceName, command.ServerIpAddress, command.ServerPort);
            var djobServiceRepository = uow.GetSimpleRepository<DJobRemoteService>();
            var dService = await djobServiceRepository.Query()
                .Where(t => t.ServerIpAddress == command.ServerIpAddress && t.ServerPort == command.ServerPort)
                .FirstOrDefaultAsync();

            if (dService == null)
            {
                dService = new DJobRemoteService(command.InstanceName, command.ServerIpAddress, command.ServerPort);
                await djobServiceRepository.CreateAsync(dService);
            }
            else
            {
                dService.UpdatedOn = DateTime.UtcNow;
            }

            var djobClient = djobClientFactory.CreateFromIpPort(command.ServerIpAddress, command.ServerPort, settings.DJobApiKey);
            bool pingOk = false;

            try
            {
                djobClient.Ping().Wait();
                pingOk = true;
            }
            catch (Exception e)
            {
                logger.LogError(e, "failed to ping DJob service {0} {1}", command.ServerIpAddress, command.ServerPort);
            }

            dService.Alive = pingOk;
            if (!pingOk) Validation.ThrowError("failed to send ping");
        }
    }
}

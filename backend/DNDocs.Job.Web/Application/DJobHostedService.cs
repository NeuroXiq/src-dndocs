
using DNDocs.Api.Client;
using DNDocs.Api.Model.Integration;
using DNDocs.Job.Web.Services;
using DNDocs.Job.Web.Shared;
using Microsoft.Extensions.Options;
using Vinca.Exceptions;

namespace DNDocs.Job.Web.Application
{
    public class DJobHostedService : IHostedService
    {
        private IBgJobsService bgjobService;

        public DJobHostedService(IBgJobsService bgjobService)
        {
            this.bgjobService = bgjobService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await bgjobService.AppStart();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await bgjobService.AppStopAsync();
        }
    }
}

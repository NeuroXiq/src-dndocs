
using DNDocs.Application.Shared;
using DNDocs.Docs.Api.Client;
using DNDocs.Domain.Enums;
using DNDocs.Domain.Service;
using DNDocs.Domain.UnitOfWork;
using DNDocs.Domain.Utils;
using DNDocs.Shared.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DNDocs.Job.Api.Client;
using Vinca.Exceptions;
using Microsoft.Extensions.Options;
using DNDocs.Shared.Configuration;
using DNDocs.Job.Api.Management;
using System.Text.Json;
using DNDocs.Domain.Entity;
using DNDocs.App.Domain.Entity;

namespace DNDocs.Application.CommandHandlers.Application
{
    internal class BuildNugetOrgProjectHandler : CommandHandlerA<BuildNugetOrgProjectCommand>
    {
        private IDNDocsJobApiClient dndocsJobApiClient;
        private DNDocsSettings settings;

        public BuildNugetOrgProjectHandler(
            IDNDocsJobApiClient dndocsJobApiClient,
            IOptions<DNDocsSettings> settings)
        {
            this.dndocsJobApiClient = dndocsJobApiClient;
            this.settings = settings.Value;
        }

        public override async Task Handle(BuildNugetOrgProjectCommand command)
        {
            while (true)
            {
                NugetOrgProject nextToBuild = await uow.GetSimpleRepository<NugetOrgProject>()
                    .Query()
                    .Include(t => t.NugetPackage)
                    .Where(t => t.State == NugetOrgProjectState.WaitingToBuild)
                    .OrderBy(t => t.CreatedOn)
                    .FirstOrDefaultAsync();

                if (nextToBuild == null) break;

                await SendBuildProjectAsync(nextToBuild);
            }
        }

        private int requestsCounter = 0;

        private IDNDocsJobApiClient[] djobClients = null;

        private async Task SendBuildProjectAsync(NugetOrgProject nextToBuild)
        {
            bool retry = true;
            bool success = false;
            IDNDocsJobApiClient nextClient = null;
            BuildNugetOrgProjectModel model = null;

            do
            {
                requestsCounter++;

                try
                {
                    model = new BuildNugetOrgProjectModel()
                    {
                        ProjectId = nextToBuild.Id,
                        PackageName = nextToBuild.NugetPackage.IdentityId,
                        PackageVersion = nextToBuild.NugetPackage.IdentityVersion
                    };

                    if (requestsCounter % 20 == 1) await FromTimeToTimeRevalidateIfClientsStillAlive();

                    nextClient = djobClients[requestsCounter % djobClients.Length];

                    nextToBuild.State = NugetOrgProjectState.Building;
                    nextToBuild.BuildStartOn = DateTime.UtcNow;
                    
                    await uow.SaveChangesAsync();

                    await nextClient.BuildNugetOrgProject(model);
                    success = true;

                    break;
                }
                catch (Exception ex)
                {
                    var e = ex as HttpRequestException;

                    if (e?.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        logger.LogWarning(e, "dndocsjob too many requests");
                        await Task.Delay(10000);
                    }
                    else
                    {
                        logger.LogError(e, "failed to request build project - bad request, data:\r\n{0}", JsonSerializer.Serialize(model));

                        retry = false;
                        retry = false;
                    }
                }
            } while (retry);

            if (success == false)
            {
                nextToBuild.State = NugetOrgProjectState.BuildFailed;

                await uow.SaveChangesAsync();
            }
        }

        // in future: this assumes multiple clients exists but there is only 1 instance of dndocs-job
        // assume there is only 1 instance and make this to work only with 1 instance
        private async Task FromTimeToTimeRevalidateIfClientsStillAlive()
        {
            try
            {
                await dndocsJobApiClient.PingAsync();
            }
            catch (Exception e)
            {

                logger.LogError(e, "dndocs job ping failed");
            }
        }
    }
}
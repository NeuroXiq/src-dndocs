
using DNDocs.Application.Commands.Projects;
using DNDocs.Application.Shared;
using DNDocs.Docs.Api.Client;
using DNDocs.Domain.Entity.App;
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

namespace DNDocs.Application.CommandHandlers.Projects
{
    internal class BuildProjectHandler : CommandHandlerA<BuildProjectCommand>
    {
        private IDJobClientFactory djobClientFactory;
        private DNDocsSettings settings;

        public BuildProjectHandler(
            IDJobClientFactory djobClientFactory,
            IOptions<DNDocsSettings> settings)
        {
            this.djobClientFactory = djobClientFactory;
            this.settings = settings.Value;
        }

        public override async Task Handle(BuildProjectCommand command)
        {
            while (true)
            {
                Project nextToBuild = await uow.ProjectRepository.Query()
                    .Where(t => t.StateDetails == ProjectStateDetails.WaitingToBuild)
                    .OrderBy(t => t.CreatedOn)
                    .FirstOrDefaultAsync();

                if (nextToBuild == null) break;

                // load related data (ef automatically binds this to 'nexttobuild')
                await uow.Query<NugetPackage>().Where(t => t.ProjectId == nextToBuild.Id).ToListAsync();

                await SendBuildProjectAsync(nextToBuild);
            }
        }

        private int requestsCounter = 0;

        private IDJobClient[] djobClients = null;

        private async Task SendBuildProjectAsync(Project nextToBuild)
        {
            bool retry = true;
            IDJobClient nextClient = null;
            BuildProjectModel model = null;

            do
            {
                requestsCounter++;

                try
                {
                    model = new BuildProjectModel()
                    {
                        ProjectId = nextToBuild.Id,
                        ProjectName = nextToBuild.ProjectName,
                        DocfxTemplate = nextToBuild.DocfxTemplate,
                        NugetOrgPackageName = nextToBuild.NugetOrgPackageName,
                        NugetOrgPackageVersion = nextToBuild.NugetOrgPackageVersion,
                        ProjectNugetPackages = nextToBuild.ProjectNugetPackages.Select(t => new BuildProjectModel.NugetPackage(t.IdentityId, t.IdentityVersion)).ToList(),
                        ProjectType = (Job.Api.Management.ProjectType)nextToBuild.ProjectType,
                        UrlPrefix = nextToBuild.UrlPrefix
                    };

                    if (requestsCounter % 20 == 1) await FromTimeToTimeRevalidateIfClientsStillAlive();

                    nextClient = djobClients[requestsCounter % djobClients.Length];

                    nextToBuild.StateDetails = ProjectStateDetails.Building;
                    nextToBuild.LastBuildStartOn = DateTime.UtcNow;
                    nextToBuild.LastBuildCompletedOn = null;
                    await uow.SaveChangesAsync();

                    await nextClient.BuildProject(model);

                    break;
                }
                catch (Exception ex)
                {
                    var e = ex as HttpRequestException;

                    if (e?.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        logger.LogWarning(e, "warning build project - too many requests: {0}", nextClient?.ServerUrl);
                        await Task.Delay(10000);
                    }
                    else if (e?.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        logger.LogError(e, "failed to request build project - bad request: {0} \r\n data: {1}", nextClient?.ServerUrl, JsonSerializer.Serialize(model));

                        nextToBuild.LastBuildErrorLog = Helpers.ExceptionToStringForLogs(e);
                        nextToBuild.StateDetails = ProjectStateDetails.BuildFailed;

                        await uow.SaveChangesAsync();

                        retry = false;
                    }
                    else
                    {
                        logger.LogError(e, "failed to request build project with unknown error. ServerUrl: {0} {1}", nextClient?.ServerUrl, JsonSerializer.Serialize(model));

                        nextToBuild.LastBuildErrorLog = Helpers.ExceptionToStringForLogs(e);
                        nextToBuild.StateDetails = ProjectStateDetails.BuildFailed;
                        await uow.SaveChangesAsync();

                        retry = false;
                        await FromTimeToTimeRevalidateIfClientsStillAlive();
                    }
                }
            } while (retry);
        }

        private async Task FromTimeToTimeRevalidateIfClientsStillAlive()
        {
            var djobservices = await uow.GetSimpleRepository<DJobRemoteService>().Query()
                .Where(t => t.Alive)
                .ToArrayAsync();

            foreach (var s in djobservices) s.Alive = false;

            var djobclients = djobservices
                .Select(s => djobClientFactory.CreateFromIpPort(s.ServerIpAddress, s.ServerPort, settings.DJobApiKey))
                .ToArray();

            List<IDJobClient> aliveClients = new List<IDJobClient>();

            for (int i = 0; i < djobservices.Length; i++)
            {
                try
                {
                    await djobclients[i].Ping();
                    djobservices[i].Alive = true;
                    aliveClients.Add(djobclients[i]);
                }
                catch
                {
                    djobservices[i].Alive = false;
                }
            }

            await uow.SaveChangesAsync();

            if (djobservices.Length == 0 || djobservices.All(t => !t.Alive)) VValidate.AppEx("all clients not alive");

            djobClients = aliveClients.ToArray();
        }
    }
}
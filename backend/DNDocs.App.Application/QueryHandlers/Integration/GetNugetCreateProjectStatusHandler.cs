
using DNDocs.Application.Shared;
using DNDocs.Domain.UnitOfWork;
using DNDocs.Api.DTO;
using DNDocs.Application.Queries.Integration;
using DNDocs.Api.DTO.MyAccount;
using DNDocs.Infrastructure.Utils;
using Microsoft.EntityFrameworkCore;
using DNDocs.Application.CommandHandlers.Integration;
using DNDocs.Application.Application;
using DNDocs.Domain.Utils;
using Microsoft.Extensions.Options;
using DNDocs.Shared.Configuration;
using Newtonsoft.Json;
using DNDocs.Application.Services;
using Microsoft.Extensions.Caching.Memory;
using DNDocs.Domain.Enums;
using DNDocs.Domain.Entity;
using DNDocs.App.Domain.Entity;

namespace DNDocs.Application.QueryHandlers.DocfxExplorer
{
    internal class GetNugetCreateProjectStatusHandler: QueryHandlerA<GetNugetCreateProjectStatusQuery, BgJobViewModel>
    {
        private IMemoryCache memoryCache;
        private DNDocsSettings settings;
        private IAppUnitOfWork appUow;
        private ICache cache;
        private ApiBackgroundWorker apiBackgroundWorker;
        private ApiBackgroundWorker abw;
        private IBgJobQueue bgjobQueue;

        public GetNugetCreateProjectStatusHandler(
            IAppUnitOfWork appUow,
            ICache cache,
            ApiBackgroundWorker apiBackgroundWorker,
            IOptions<DNDocsSettings> dsettings,
            ApiBackgroundWorker abw,
            IBgJobQueue bgjobQueue,
            IMemoryCache memoryCache)
        {
            this.memoryCache = memoryCache;
            this.settings = dsettings.Value;
            this.appUow = appUow;
            this.cache = cache;
            this.apiBackgroundWorker = apiBackgroundWorker;
            this.abw = abw;
            this.bgjobQueue = bgjobQueue;
        }

        protected override async Task<BgJobViewModel> Handle(GetNugetCreateProjectStatusQuery query)
        {
            var project = await appUow.NugetOrgProjectRepository.Query()
                .Include(t => t.NugetPackage)
                .Where(t =>
                    t.NugetPackage.IdentityId == query.PackageName &&
                    t.NugetPackage.IdentityVersion == query.PackageVersion)
                .FirstOrDefaultAsync();

            if (project == null) return null;

            int countBeforeStart = 0;
            double estimateBuildTime = await GetEstimateBuildTime();
            double estimateStartIn = 0;

            if (project.State == Domain.Enums.NugetOrgProjectState.WaitingToBuild)
            {
                countBeforeStart = await appUow.Query<NugetOrgProject>()
                    .Where(t => t.CreatedOn < project.CreatedOn && t.State == Domain.Enums.NugetOrgProjectState.WaitingToBuild)
                    .CountAsync();

                countBeforeStart++;

                estimateStartIn = countBeforeStart * estimateBuildTime;
            }

            var result = new BgJobViewModel
            {
                ProjectId = project.Id,
                EstimateOtherJobsBeforeThis = countBeforeStart,
                EstimateBuildTime = estimateBuildTime,
                EstimateStartIn = estimateStartIn,
                State = (int)project.State,
                ProjectApiFolderUrl = 
                settings.GetUrlNugetOrgProject(
                    project.NugetPackage.IdentityId,
                    project.NugetPackage.IdentityVersion),
            };

            abw.RunBuildProjects();

            return result;
        }

        async Task<double> GetEstimateBuildTime()
        {
            return 10;
        }
    }
}

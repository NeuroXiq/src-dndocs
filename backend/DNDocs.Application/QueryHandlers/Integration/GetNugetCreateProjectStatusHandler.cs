
using DNDocs.Application.Shared;
using DNDocs.Domain.UnitOfWork;
using DNDocs.Api.DTO;
using DNDocs.Application.Queries.Integration;
using DNDocs.Api.DTO.MyAccount;
using DNDocs.Domain.Entity.App;
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
            Thread.Sleep(1000);
            
            //return new BgJobViewModel
            //{
            //    ProjectId = 123,
            //    EstimateOtherJobsBeforeThis = 1,
            //    EstimateBuildTime = 12,
            //    EstimateStartIn = 32,
            //    State = (int)2,
            //    StateDetails = (int)ProjectStateDetails.BuildFailed,
            //    LastDocfxBuildTime = DateTime.Now,
            //    ProjectApiFolderUrl = "url", //settings.GetUrlNugetOrgProject(project.NugetOrgPackageName, project.NugetOrgPackageVersion),
            //};

            var project = await appUow.ProjectRepository.GetNugetOrgProjectAsync(query.PackageName, query.PackageVersion);

            if (project == null) return null;

            int countBeforeStart = 0;
            double estimateBuildTime = await GetEstimateBuildTime();
            double estimateStartIn = 0;

            if (project.State == Domain.Enums.ProjectState.NotActive && project.StateDetails == Domain.Enums.ProjectStateDetails.WaitingToBuild)
            {
                countBeforeStart = await appUow.Query<Project>()
                    .Where(t => t.CreatedOn < project.CreatedOn && t.StateDetails == Domain.Enums.ProjectStateDetails.WaitingToBuild)
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
                StateDetails = (int)project.StateDetails,
                LastDocfxBuildTime = project.LastBuildStartOn,
                ProjectApiFolderUrl = settings.GetUrlNugetOrgProject(project.NugetOrgPackageName, project.NugetOrgPackageVersion),
            };

            abw.DoSystemWorkNow();

            return result;
        }

        async Task<double> GetEstimateBuildTime()
        {
            string key = "estimated_build_time";
            if (!memoryCache.TryGetValue<double>(key, out var value))
            {
                var last50SuccessBuilds = await appUow.Query<Project>()
                    .Where(t => t.State == Domain.Enums.ProjectState.Active && t.LastBuildStartOn != null && t.LastBuildCompletedOn != null)
                    .OrderByDescending(t => t.LastBuildStartOn)
                    .Take(50)
                    .ToListAsync();

                if (last50SuccessBuilds.Count == 0) return 0;

                var estimatedBuildTime = last50SuccessBuilds.Average(t => (t.LastBuildCompletedOn.Value - t.LastBuildStartOn.Value).TotalSeconds);
                estimatedBuildTime = Math.Round(estimatedBuildTime, 2);

                memoryCache.Set(key, estimatedBuildTime);
                value = estimatedBuildTime;
            }

            return value;
        }
    }
}

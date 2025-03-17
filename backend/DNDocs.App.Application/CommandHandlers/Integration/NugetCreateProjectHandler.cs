using DNDocs.Application.Commands.Integration;
using DNDocs.Application.Shared;
using DNDocs.Domain.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DNDocs.Domain.ValueTypes;
using DNDocs.Domain.Utils;
using Microsoft.Extensions.Caching.Memory;
using DNDocs.Infrastructure.Utils;
using DNDocs.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using DNDocs.Application.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DNDocs.Application.Services;
using Vinca.Api.Nuget;
using DNDocs.Domain.Entity;
using DNDocs.App.Domain.Entity;
using DNDocs.Domain.Repository;
using Vinca.Exceptions;

namespace DNDocs.Application.CommandHandlers.Integration
{
    internal class NugetCreateProjectHandler : CommandHandlerA<NugetCreateProjectCommand>
    {
        private INugetRepositoryFacade nugetRepositoryFacade;
        private ICache cache;
        private IAppUnitOfWork appUow;
        private IBgJobQueue bgjobQueue;
        private ApiBackgroundWorker bgw;
        private IRepository<NugetOrgProject> nugetOrgProjectRepository;

        public NugetCreateProjectHandler(
            INugetRepositoryFacade nugetRepositoryFacade,
            ICache cache,
            IAppUnitOfWork appUow,
            IBgJobQueue bgjobQueue,
            ApiBackgroundWorker bgw)
        {
            this.nugetRepositoryFacade = nugetRepositoryFacade;
            this.cache = cache;
            this.appUow = appUow;
            this.bgjobQueue = bgjobQueue;
            this.bgw = bgw;
            this.nugetOrgProjectRepository = appUow.GetSimpleRepository<NugetOrgProject>();
        }

        public override async Task Handle(NugetCreateProjectCommand command)
        {
            var packageName = command.PackageName;
            var packageVersion = command.PackageVersion;
            logger.LogInformation("starting to create nuget project: {0} {1}", packageName, packageVersion);

            Validation.NotStringIsNullOrWhiteSpace(packageName, "PackageName is empty");
            Validation.NotStringIsNullOrWhiteSpace(packageVersion, "PackageVersion is empty");

            var existing = await appUow.Query<NugetOrgProject>()
                .Where(t => t.NugetPackage.IdentityId == packageName && t.NugetPackage.IdentityVersion == packageVersion)
                .FirstOrDefaultAsync();

            var cacheKey = $"NugetPackageMetadata_{command.PackageName}";
            var packagesMetadata = await cache.TryGetDbAsync<PackageSearchMetadata[]>(cacheKey);

            // if no cache or if not exists cache reload cache
            if (packagesMetadata == null || !packagesMetadata.Any(t => t.IdentityId == packageName && t.IdentityVersion == packageVersion))
            {
                try
                {
                    packagesMetadata = await nugetRepositoryFacade.GetPackageMetadataAsync(command.PackageName);
                    await cache.SetDbAsync(cacheKey, packagesMetadata, TimeSpan.FromDays(14));
                }
                catch
                {
                    Validation.ThrowError($"Failed to fetch nuget package: {packageName} {packageVersion}");
                }
            }

            var nugetPackageData = packagesMetadata.First(t => t.IdentityId == packageName && t.IdentityVersion == packageVersion);

            if (!packagesMetadata.Any(t => t.IdentityId == packageName && t.IdentityVersion == packageVersion))
            {
                Validation.ThrowError($"Failed to fetch nuget package: {packageName} {packageVersion}");
            }

            if (existing == null)
            {
                var nugetPackage = new NugetPackage(
                    nugetPackageData.Title,
                    nugetPackageData.IdentityId,
                    nugetPackageData.IdentityVersion,
                    nugetPackageData.Published,
                    nugetPackageData.ProjectUrl,
                    nugetPackageData.PackageDetailsUrl,
                    nugetPackageData.IsListed);

                var nugetProject = new NugetOrgProject(nugetPackage, Domain.Enums.NugetOrgProjectState.WaitingToBuild);
                await nugetOrgProjectRepository.CreateAsync(nugetProject);
            }
            else if (existing != null)
            {
                if (existing.State != Domain.Enums.NugetOrgProjectState.BuildFailed)
                {
                    Validation.ThrowError(existing != null, $"nuget project '{packageName} {packageVersion}' already exists");
                }

                existing.State = Domain.Enums.NugetOrgProjectState.WaitingToBuild;
            }

            await appUow.SaveChangesAsync();

            bgw.RunBuildProjects();
        }
    }
}
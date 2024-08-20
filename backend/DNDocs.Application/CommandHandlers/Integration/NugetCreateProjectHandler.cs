using DNDocs.Application.Commands.Integration;
using DNDocs.Application.Shared;
using DNDocs.Domain.Service;
using DNDocs.Domain.ValueTypes.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DNDocs.Domain.ValueTypes;
using DNDocs.Domain.ValueTypes.Project;
using DNDocs.Domain.Utils;
using Microsoft.Extensions.Caching.Memory;
using DNDocs.Infrastructure.Utils;
using DNDocs.Domain.UnitOfWork;
using DNDocs.Domain.Entity.App;
using Microsoft.EntityFrameworkCore;
using DNDocs.Application.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DNDocs.Application.Services;
using DNDocs.Application.Commands.Projects;
using Vinca.Api.Nuget;

namespace DNDocs.Application.CommandHandlers.Integration
{
    internal class NugetCreateProjectHandler : CommandHandlerA<NugetCreateProjectCommand>
    {
        private IProjectManager projectManager;
        private INugetRepositoryFacade nugetRepositoryFacade;
        private ICache cache;
        private IAppUnitOfWork appUow;
        private IBgJobQueue bgjobQueue;
        private ApiBackgroundWorker bgw;

        public NugetCreateProjectHandler(
            IProjectManager projectManager,
            INugetRepositoryFacade nugetRepositoryFacade,
            ICache cache,
            IAppUnitOfWork appUow,
            IBgJobQueue bgjobQueue,
            ApiBackgroundWorker bgw)
        {
            this.projectManager = projectManager;
            this.nugetRepositoryFacade = nugetRepositoryFacade;
            this.cache = cache;
            this.appUow = appUow;
            this.bgjobQueue = bgjobQueue;
            this.bgw = bgw;
        }

        public override async Task Handle(NugetCreateProjectCommand command)
        {
            Thread.Sleep(1000);
            //return;
            var packageName = command.PackageName;
            var packageVersion = command.PackageVersion;
            logger.LogInformation("starting to create nuget project: {0} {1}", packageName, packageVersion);

            Validation.NotStringIsNullOrWhiteSpace(packageName, "PackageName is empty");
            Validation.NotStringIsNullOrWhiteSpace(packageVersion, "PackageVersion is empty");

            var existing = await appUow.ProjectRepository.GetNugetOrgProjectAsync(packageName, packageVersion);

            if (existing != null)
            {
                // force run again?
                if (existing.StateDetails == Domain.Enums.ProjectStateDetails.BuildFailed)
                {
                    await appUow.ProjectRepository.DeleteAsync(existing.Id);
                }
                else
                {
                    Validation.ThrowError(existing != null, $"nuget project '{packageName} {packageVersion}' already exists");
                }
            }

            User nugetUser = await appUow.UserRepository.GetByLoginAsync(User.NuGetUserLogin);

            Project p = new Project();
            p.NugetOrgPackageName = packageName;
            p.NugetOrgPackageVersion = packageVersion;
            p.ProjectType = ProjectType.NugetOrg;
            p.ProjectName = $"{packageName} {packageVersion}";
            p.CreatedOn = DateTime.Now;
            p.State = Domain.Enums.ProjectState.NotActive;
            p.StateDetails = Domain.Enums.ProjectStateDetails.WaitingToBuild;
            p.AddUser(nugetUser);
            p.ProjectNugetPackages = await projectManager.ValidateAndCreateNugetPackages(new NugetPackageDto[] { new NugetPackageDto(packageName, packageVersion) }, false);

            int nugetUserId = await cache.GetOrAddOKMAsync(
                this,
                "nuget-user-login",
                async () => (await appUow.UserRepository.GetByLoginAsync(User.NuGetUserLogin)).Id,
                TimeSpan.FromDays(1));

            Validation.ThrowError(await (appUow.ProjectRepository.GetByNameAsync(p.ProjectName)) != null, "project already exists/queued to process");

            await appUow.ProjectRepository.CreateAsync(p);
            await appUow.SaveChangesAsync();

            bgw.DoSystemWorkNow();
        }
    }
}
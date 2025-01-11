using DNDocs.Application.Commands.Application;
using DNDocs.Application.Shared;
using DNDocs.Domain.Service;
using DNDocs.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vinca.Api.Nuget;
using Vinca.Utils;

namespace DNDocs.Application.CommandHandlers.Application
{
    class BgJobProcessNugetCatalogHandler : CommandHandlerA<BgJobProcessNugetCatalogCommand>
    {
        INugetRepositoryFacade nugetRepo;
        IProjectManager projectManager;

        public BgJobProcessNugetCatalogHandler(
            INugetRepositoryFacade nugetRepo,
            IProjectManager projectManager)
        {
            this.nugetRepo = nugetRepo;
            this.projectManager = projectManager;
        }

        public override async Task Handle(BgJobProcessNugetCatalogCommand command)
        {
            // fetch data from nuget catalog to check which packages were deleted
            // and block/delete them (e.g. https://github.com/NuGet/NuGetGallery/issues/10055) 
            var maxOldToCheck = DateTime.Now.AddDays(-30);

            var pagesInDb = await uow.Query<DNDocs.Domain.Entity.App.NugetCatalogPage>()
                .Where(t => t.CommitTimeStamp > maxOldToCheck)
                .OrderByDescending(t => t.CommitTimeStamp)
                .ToListAsync();

            var catalogItems = (await nugetRepo.GetCatalogRoot()).Items;

            foreach (var pageInDb in pagesInDb)
            {
                // was modified?
                if (catalogItems.FirstOrDefault(t => t.Id == pageInDb.NId)?.CommitId != pageInDb.CommitId)
                {
                    // yes so will need to fetch newest data and process again
                    pageInDb.State = Domain.Enums.NugetCatalogPageState.ToProcess;
                }
            }

            // get pages not in db (date > max date in db) or if nothing in db then take anything using fallback value
            var newPagesDate = pagesInDb.FirstOrDefault()?.CommitTimeStamp ?? DateTime.Now.AddDays(-10);
            var newPages = catalogItems
                .Where(t => t.CommitTimeStamp > newPagesDate)
                .Select(t => new Domain.Entity.App.NugetCatalogPage(t.Id, t.CommitId, t.CommitTimeStamp, t.Count))
                .ToList();

            logger.LogInformation("new nuget catalog pages to insert to db:\r\n{0}", newPages.StringJoin(",\r\n", t => t.NId));

            await uow.GetSimpleRepository<Domain.Entity.App.NugetCatalogPage>().CreateAsync(newPages);
            await uow.SaveChangesAsync();

            var toProcess = uow.Query<Domain.Entity.App.NugetCatalogPage>()
                .Where(t => t.State == Domain.Enums.NugetCatalogPageState.ToProcess)
                .ToList();

            logger.LogInformation("nuget catalog pages to process:\r\n{0}", toProcess.StringJoin("\r\n", t => t.NId));


            foreach (var p in toProcess)
            {
                try
                {
                    logger.LogTrace("starting to fetch nuget catalog page: Id: {0}, NId: {1}", p.Id, p.NId);
                    var page = await nugetRepo.GetCatalogPage(p.NId);
                    var deletedItems = page.Items.Where(t => t.Type == "nuget:PackageDelete").ToList();

                    foreach (var deletedItem in deletedItems)
                    {
                        logger.LogTrace("starting to check db for package to delete: {0} {1} {2}", deletedItem.NugetId, deletedItem.NugetVersion, deletedItem.Id);

                        var projectToDelete = await uow.ProjectRepository.Query()
                            .Where(t =>
                                t.NugetOrgPackageName == deletedItem.NugetId &&
                                t.NugetOrgPackageVersion == deletedItem.NugetVersion)
                            .FirstOrDefaultAsync();

                        if (projectToDelete != null)
                        {
                            logger.LogTrace("starting to delete: {0} {1} {2}", deletedItem.NugetId, deletedItem.NugetVersion, deletedItem.Id);
                            await projectManager.DeleteProject(projectToDelete.Id);
                            logger.LogTrace("success delete: {0} {1} {2}", deletedItem.NugetId, deletedItem.NugetVersion, deletedItem.Id);
                        }
                    }

                    p.State = Domain.Enums.NugetCatalogPageState.Done;
                    await uow.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    logger.LogError(e, "failed to process nuget catalog page");
                }
            }
        }
    }
}

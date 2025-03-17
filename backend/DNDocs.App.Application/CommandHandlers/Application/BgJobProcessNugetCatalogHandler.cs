using DNDocs.App.Domain.Service;
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
        private INugetOrgProjectService nugetOrgProjectService;

        public BgJobProcessNugetCatalogHandler(
            INugetRepositoryFacade nugetRepo,
            INugetOrgProjectService nugetOrgProjectService)
        {
            this.nugetRepo = nugetRepo;
            this.nugetOrgProjectService = nugetOrgProjectService;
        }

        public override async Task Handle(BgJobProcessNugetCatalogCommand command)
        {
            // fetch data from nuget catalog to check which packages were deleted/modified
            // 1. fetch page if not in our DB now
            // 2. fetch page if nuget changed that page and process again
            // then process: 
            // 1. block/delete packages (e.g. https://github.com/NuGet/NuGetGallery/issues/10055) 
            // 2. generate newest nuget package peemptively if it was changed and we already host it
            var maxOldToCheck = DateTime.Now.AddDays(-30);

            var pagesInDb = await uow.Query<Domain.Entity.NugetCatalogPage>()
                .Where(t => t.CommitTimeStamp > maxOldToCheck)
                .OrderByDescending(t => t.CommitTimeStamp)
                .ToListAsync();

            var catalogItems = (await nugetRepo.GetCatalogRootAsync()).Items;

            foreach (var pageInDb in pagesInDb)
            {
                // we have processed some nuget catalog page earlier, 
                // was this page modified by nuget later?
                if (catalogItems.FirstOrDefault(t => t.Id == pageInDb.NId)?.CommitId != pageInDb.CommitId)
                {
                    // yes so we will need to fetch newest data and process again
                    pageInDb.State = Domain.Enums.NugetCatalogPageState.ToProcess;
                }
            }

            // get pages not in db (date > max date in db) or if nothing in db then take anything using fallback value (-10 days)
            var newPagesDate = pagesInDb.FirstOrDefault()?.CommitTimeStamp ?? DateTime.Now.AddDays(-10);
            var newPages = catalogItems
                .Where(t => t.CommitTimeStamp > newPagesDate)
                .Select(t => new Domain.Entity.NugetCatalogPage(t.Id, t.CommitId, t.CommitTimeStamp, t.Count))
                .ToList();

            logger.LogInformation("new nuget catalog pages to insert to db:\r\n{0}", newPages.StringJoin(",\r\n", t => t.NId));

            await uow.GetSimpleRepository<Domain.Entity.NugetCatalogPage>().CreateAsync(newPages);
            await uow.SaveChangesAsync();

            var toProcess = uow.Query<Domain.Entity.NugetCatalogPage>()
                .Where(t => t.State == Domain.Enums.NugetCatalogPageState.ToProcess)
                .ToList();

            logger.LogInformation("nuget catalog pages to process:\r\n{0}", toProcess.StringJoin("\r\n", t => t.NId));

            foreach (var p in toProcess)
            {
                try
                {
                    logger.LogTrace("starting to fetch nuget catalog page: Id: {0}, NId: {1}", p.Id, p.NId);
                    var page = await nugetRepo.GetCatalogPageAsync(p.NId);
                    var deletedItems = page.Items.Where(t => t.Type == "nuget:PackageDelete").ToList();

                    foreach (var deletedItem in deletedItems)
                    {
                        logger.LogTrace("starting to check db for package to delete: {0} {1} {2}", deletedItem.NugetId, deletedItem.NugetVersion, deletedItem.Id);

                        var projectToDelete = await uow.NugetOrgProjectRepository.Query()
                            .Where(t =>
                                t.NugetPackage.IdentityId == deletedItem.NugetId &&
                                t.NugetPackage.IdentityVersion == deletedItem.NugetVersion)
                            .FirstOrDefaultAsync();

                        if (projectToDelete != null)
                        {
                            logger.LogTrace("starting to delete: {0} {1} {2}", deletedItem.NugetId, deletedItem.NugetVersion, deletedItem.Id);
                            await nugetOrgProjectService.DeleteAsync(projectToDelete.Id);
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

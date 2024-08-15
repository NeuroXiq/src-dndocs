using DNDocs.Docs.Web.Model;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System.Diagnostics;

namespace DNDocs.Docs.Web.Services
{
    public interface IDMemCache
    {
        Task<SiteItem> GetSiteItem(long projectId, string path);
        Task<Project> GetNugetProject(string nugetPackageName, string nugetPackageVersion);
        Task<PublicHtml> GetPublicHtmlFile(string path);
        Task<Project> GetSingletonProject(string urlPrefix);
    }

    public class DMemCache : IDMemCache
    {
        private IQRepository repository;
        private IMemoryCache memoryCache;
        private IDMetrics metrics;

        public DMemCache(IMemoryCache memoryCache, IQRepository repository, IDMetrics metrics)
        {
            this.repository = repository;
            this.memoryCache = memoryCache;
            this.metrics = metrics;
        }

        private bool TryGetValue<T>(string key, out T value)
        {
            bool result = memoryCache.TryGetValue<T>(key, out value);

            if (result) metrics.CacheHit();
            else metrics.CacheMiss();

            return result;
        }

        public async Task<PublicHtml> GetPublicHtmlFile(string path)
        {
            string key = $"publichtml_{path}";
            if (!TryGetValue<PublicHtml>(key, out var publicHtml))
            {
                publicHtml = await repository.SelectPublicHtml(path);
                
                if (publicHtml == null) return null;

                memoryCache.Set(key, publicHtml, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
                    Size = publicHtml.ByteData?.Length ?? 1,
                    SlidingExpiration = TimeSpan.FromMinutes(15)
                });
            }

            return publicHtml;
        }

        public async Task<SiteItem> GetSiteItem(long projectId, string path)
        {
            string siteItemKey = $"si_{projectId}_{path}".ToLower();

            SiteItem siteItem = null;

            if (!TryGetValue(siteItemKey, out siteItem))
            {
                siteItem = await repository.SelectSiteItemAsync(projectId, path);

                if (siteItem == null) return null;

                memoryCache.Set(siteItemKey, siteItem, new MemoryCacheEntryOptions()
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(45),
                    SlidingExpiration = TimeSpan.FromMinutes(30),
                    Size = siteItem.ByteData?.Length ?? 1
                });
            }

            if (siteItem.SharedSiteItemId.HasValue)
            {
                string ssiKey = $"ssi_{siteItem.SharedSiteItemId.Value}".ToLower();

                SharedSiteItem sharedSiteItem = null;

                if (!TryGetValue(ssiKey, out sharedSiteItem))
                {
                    sharedSiteItem = await repository.SelectSharedSiteItem(siteItem.SharedSiteItemId.Value);

                    memoryCache.Set(ssiKey, sharedSiteItem, new MemoryCacheEntryOptions()
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60),
                        SlidingExpiration = TimeSpan.FromMinutes(10),
                        Size = sharedSiteItem.ByteData.Length
                    });
                }

                siteItem.ByteData = sharedSiteItem.ByteData;
            }

            return siteItem;
        }

        public async Task<Project> GetNugetProject(string packageName, string packageVersion)
        {
            string projectKey = $"p_{ProjectType.Nuget}_{packageName}_{packageVersion}".ToLower();

            if (!TryGetValue<Project>(projectKey, out var project))
            {
                project = await repository.SelectNugetProjectAsync(packageName, packageVersion);

                memoryCache.Set(projectKey, project, new MemoryCacheEntryOptions()
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60),
                    SlidingExpiration = TimeSpan.FromMinutes(30),
                    Size = 10
                });
            }

            return project;
        }

        public async Task<Project> GetSingletonProject(string urlPrefix)
        {
            return await repository.SelectSingletonProjectAsync(urlPrefix);
        }
    }
}

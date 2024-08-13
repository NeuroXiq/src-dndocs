using Microsoft.Extensions.Logging;
using NuGet.Common;
using NuGet.Packaging;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vinca.Exceptions;
using Vinca.Utils;

namespace Vinca.Api.Nuget
{
    public interface INugetRepositoryFacade
    {
        PackageLibFile[] FetchDllAndXmlFromPackage(string packageName, string version);
        PackageSearchMetadata GetLatestPackage(string packageName);
        PackageSearchMetadata GetPackageMetadata(string identityId, string identityVersion);
        PackageSearchMetadata[] GetPackageMetadata(string packageName);
    }

    public class PackageSearchMetadata
    {
        public string Title { get; set; }
        public string IdentityVersion { get; set; }
        public string IdentityId { get; set; }
        public DateTimeOffset? Published { get; set; }
        public string ProjectUrl { get; set; }
        public string PackageDetailsUrl { get; set; }
        public bool IsListed { get; set; }

        public PackageSearchMetadata(string title,
            string identityVersion,
            string identityId,
            DateTimeOffset? published,
            string projectUrl,
            string packageDetailsUrl,
            bool isListed)
        {
            Title = title;
            IdentityVersion = identityVersion;
            IdentityId = identityId;
            Published = published;
            ProjectUrl = projectUrl;
            PackageDetailsUrl = packageDetailsUrl;
            IsListed = isListed;
        }
    }

    public class PackageLibFile
    {
        public string FileName { get; set; }
        public string PackageRelativePath { get; set; }
        public byte[] ByteData { get; set; }
    }

    internal class NugetRepositoryFacade : INugetRepositoryFacade
    {
        private IOSApi osapi;
        private ILogger<NugetRepositoryFacade> logger;

        // private ICache cache;
        // private IAppManager appManager;

        public NugetRepositoryFacade(IOSApi osapi, ILogger<NugetRepositoryFacade> logger)
        {
            this.osapi = osapi;
            this.logger = logger;
        }

        public PackageLibFile[] FetchDllAndXmlFromPackage(string packageName, string version)
        {
            logger.LogTrace("FetchDllAndXmlFromPackage({0},{1})", packageName, version);

            try
            {
                var task = GetDllAndXmlFromPackageAsync(packageName, version);
                task.Wait();

                if (task.Exception != null) throw task.Exception;

                return task.Result;
            }
            catch (Exception e)
            {
                logger.LogError(e, "failed to fetch dll and xml from nuget package: {0} {1}", packageName, version);
                throw;
            }

            return null;
        }

        public PackageSearchMetadata GetPackageMetadata(string packageName, string packageVersion)
        {
            VValidate.AppEx(string.IsNullOrWhiteSpace(packageName), "packagename null or empty");
            VValidate.AppEx(string.IsNullOrWhiteSpace(packageVersion), "packageVersion null or empty");

            var allMetadata = GetPackageMetadata(packageName);
            var result = allMetadata.FirstOrDefault(t => t.IdentityVersion == packageVersion);

            VValidate.AppEx(result == null, $"Package '{packageName} {packageVersion}' not found. Current packages found:\r\n{allMetadata.StringJoin("\r\n", t => $"{t.IdentityId} {t.IdentityVersion}")}");

            return result;
        }

        public PackageSearchMetadata GetLatestPackage(string packageName)
        {
            var allMetadata = GetPackageMetadata(packageName);

            VValidate.Throw(allMetadata.Length == 0, $"Package does not exists: '{packageName}'");

            var latest = allMetadata.Where(t => !string.IsNullOrWhiteSpace(t.IdentityVersion))
                .OrderByDescending(t => t.IdentityVersion)
                .FirstOrDefault();

            latest = latest ?? allMetadata.Last();
            var p = latest;

            return p;
        }

        public PackageSearchMetadata[] GetPackageMetadata(string packageName)
        {
            var allMetadata = GetNugetPackageMetadata(packageName);

            // if (cache.TryGetOKM<Domain.ValueTypes.PackageSearchMetadata[]>(this, packageName, out var cached)) return cached;

            var result = allMetadata.Select(p => new PackageSearchMetadata(
                p.Title,
                p.Identity.HasVersion ? p.Identity.Version.ToString() : null,
                p.Identity.Id,
                p.Published,
                p.ProjectUrl?.ToString(),
                p.PackageDetailsUrl?.ToString(),
                p.IsListed))
                .ToArray();

            // cache.AddOKM(this, packageName, result, TimeSpan.FromMinutes(10));

            return result;
        }

        private IPackageSearchMetadata[] GetNugetPackageMetadata(string packageName)
        {
            // string cachekey = cache.Key(nameof(NugetRepositoryFacade), nameof(GetPackageMetadata), packageName);

            // if (cache.TryGetOKM<IPackageSearchMetadata[]>(this, packageName, out var cachedValue)) return cachedValue;

            var t = GetPackageMetadataAsync(packageName);

            t.Wait();

            if (t.Exception != null) throw t.Exception;
            var result = t.Result;

            // cache.AddOKM(this, packageName, result, TimeSpan.FromMinutes(15));

            return result;
        }

        private async Task<PackageLibFile[]> GetDllAndXmlFromPackageAsync(string packageName, string version)
        {
            var logger = NullLogger.Instance;
            CancellationToken cancellationToken = CancellationToken.None;

            SourceCacheContext cache = new SourceCacheContext();
            SourceRepository repository = NuGet.Protocol.Core.Types.Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
            FindPackageByIdResource resource = await repository.GetResourceAsync<FindPackageByIdResource>();

            string packageId = packageName;
            NuGetVersion packageVersion = new NuGetVersion(version);

            using MemoryStream packageStream = new MemoryStream();

            await resource.CopyNupkgToStreamAsync(
                packageId,
                packageVersion,
                packageStream,
                cache,
                logger,
                cancellationToken);

            using PackageArchiveReader packageReader = new PackageArchiveReader(packageStream);
            NuspecReader nuspecReader = await packageReader.GetNuspecReaderAsync(cancellationToken);

            var items = packageReader.GetLibItems().ToArray();

            string[] knownProbablyWillWork = new string[]
            {
                "net8.0",
                "net7.0",
                "net6.0",
                "net5.0",
                "netcoreapp3.1",
                "netstandard2.1",
                "netstandard2.0",
                "netstandard20",
                "netstandard1.1",
                "netstandard1.6",
                "netstandard1.3",
                "netstandard1.0",
                "netcore50",
                "net47",
                "net463",
                "net462",
                "net46",
                "net47",
                "net45",
                "net40",
                "net35",
            };

            // maybe this is not neede?, how to select highest version?
            // maybe sort by version and select first?

            // 1.first try to find 'known' from above
            var frameworkSpecificGroup = items.FirstOrDefault(i => knownProbablyWillWork.Any(d => d == i.TargetFramework.GetShortFolderName()));

            //2. if not exists, find first 'net' or netstandard
            if (frameworkSpecificGroup == null)
            {
                frameworkSpecificGroup = items.OrderBy(t =>
                {
                    var a = t.TargetFramework.GetShortFolderName();
                    return a.Length == "netX.Y".Length && a[4] == '.';
                })
                .ThenBy(t => t.TargetFramework.GetShortFolderName().Contains("netstandard"))
                .FirstOrDefault(i =>
                {
                    var folder = i.TargetFramework.GetShortFolderName();
                    return folder.Contains("net") || folder.Contains("netstandard");
                });
            }

            List<PackageLibFile> result = new List<PackageLibFile>();

           VValidate.AppEx(frameworkSpecificGroup == null, $"Failed to find valid .NET Core folder version for package: '{packageId} {packageVersion}'");

            using (var tempFolder = osapi.CreateTempDir())
            {
                foreach (var relativeZipPath in frameworkSpecificGroup.Items)
                {
                    var filename = Path.GetFileName(relativeZipPath);

                    if (filename.EndsWith("dll") || filename.EndsWith("xml"))
                    {
                        string fileOnOSDisk = packageReader.ExtractFile(relativeZipPath, Path.Combine(tempFolder.OSFullPath, filename), logger);
                        byte[] fileData = File.ReadAllBytes(fileOnOSDisk);

                        result.Add(new PackageLibFile() { ByteData = fileData, PackageRelativePath = relativeZipPath, FileName = filename });
                    }
                }
            }

            return result.ToArray();
        }

        private async Task<IPackageSearchMetadata[]> GetPackageMetadataAsync(string packageName)
        {
            VValidate.AppEx(string.IsNullOrWhiteSpace(packageName), "null or empty");

            var logger = NullLogger.Instance;
            CancellationToken cancellationToken = CancellationToken.None;

            SourceCacheContext cache = new SourceCacheContext();
            SourceRepository repository = NuGet.Protocol.Core.Types.Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");

            PackageMetadataResource resource = await repository.GetResourceAsync<PackageMetadataResource>();

            IEnumerable<IPackageSearchMetadata> packages = await resource.GetMetadataAsync(
                packageName,
                includePrerelease: true,
                includeUnlisted: false,
                cache,
                logger,
                cancellationToken);

            return packages.ToArray();

            //foreach (IPackageSearchMetadata package in packages)
            //{
            //    Console.WriteLine($"Version: {package.Identity.Version}");
            //    Console.WriteLine($"Listed: {package.IsListed}");
            //    Console.WriteLine($"Tags: {package.Tags}");
            //    Console.WriteLine($"Description: {package.Description}");
            //}

            //return "";
        }
    }
}

using Dapper;
using DNDocs.Docs.Web.Infrastructure;
using DNDocs.Docs.Web.Model;
using DNDocs.Docs.Web.Shared;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Net.Http.Headers;
using System.Buffers;
using System.Diagnostics;
using System.IO.Compression;
using System.Security.Cryptography;
using Vinca.Exceptions;
using Vinca.Utils;

namespace DNDocs.Docs.Web.Services
{
    public interface IManagementService
    {
        Task CreateProject(
            int projectId,
            string metadata,
            string projectName,
            string urlPrefix,
            string nPackageName,
            string nPackageVersion,
            ProjectType type,
            Stream zipStream);

        public Task TryDeleteProjectByDnId(int projectDnId, ProjectType type);
        public void Ping();
    }

    public class ManagementService : IManagementService
    {
        private ILogger<ManagementService> logger;
        private IQRepository qrepository;
        private IDMetrics metrics;
        private ITxRepository repository = null;

        public ManagementService(
            ITxRepository txRepository,
            ILogger<ManagementService> logger,
            IQRepository qrepository,
            IDMetrics metrics)
        {
            this.metrics = metrics;
            this.repository = txRepository;
            this.logger = logger;
            this.qrepository = qrepository;
            this.repository.BeginTransaction();
        }

        public async Task CreateProject(
            int projectId,
            string metadata,
            string projectName,
            string urlPrefix,
            string nPackageName,
            string nPackageVersion,
            ProjectType type,
            Stream zipStream)
        {
            var sw = Stopwatch.StartNew();
            metrics.CreateProjectZipSize(zipStream.Length);

            logger.LogInformation("Starting CreateOrReplace: {0}, {1}, {2}, {3}, {4}, {5}, {6}",
                projectId, projectName, urlPrefix, nPackageName, nPackageVersion, type, metadata);

            nPackageName = string.IsNullOrEmpty(nPackageName) ? null : nPackageName;
            nPackageVersion = string.IsNullOrEmpty(nPackageVersion) ? null : nPackageVersion;

            VValidate.Throw(projectId == 0, "ProjectId == 0");
            VValidate.Throw(string.IsNullOrWhiteSpace(projectName), "projectname");
            VValidate.Throw(!Enum.IsDefined(type), "enum type not defined");
            VValidate.Throw((await repository.SelectProjectByIdAsync(projectId)) != null, $"project with project_id={projectId} already exists");
            // check unique urlprefix
            // check unique name

            switch (type)
            {
                case ProjectType.Nuget:
                    VValidate.Throw(string.IsNullOrWhiteSpace(nPackageName), "nugetackagename not null");
                    VValidate.Throw(string.IsNullOrWhiteSpace(nPackageVersion), "nugetackagevernot null");
                    VValidate.Throw(!string.IsNullOrWhiteSpace(urlPrefix), "urlprefix must be null");
                    VValidate.Throw((
                        await qrepository.SelectNugetProjectAsync(nPackageName, nPackageVersion)) != null,
                        $"project with nuget package: {nPackageName} {nPackageVersion} already exists");
                    break;
                default:
                    break;
            }

            var project = await repository.SelectProjectByIdAsync(projectId);

            project = new Project();
            project.Metadata = metadata;
            project.CreatedOn = DateTime.UtcNow;
            project.UpdatedOn = DateTime.UtcNow;
            project.DnProjectId = projectId;
            project.UrlPrefix = urlPrefix;
            project.NugetPackageName = nPackageName;
            project.NugetPackageVersion = nPackageVersion;
            project.ProjectType = type;

            await repository.InsertProjectAsync(project);

            byte[] compressBuffer = new byte[1024 * 1024];

            logger.LogTrace("Starting decompress zip archive. ProjectId {0}", projectId);
            using (ZipArchive zip = new ZipArchive(zipStream))
            {
                MemoryStream fileStream;
                var onlyFiles = zip.Entries
                    .Where(f => !f.FullName.EndsWith("/"))
                    .ToArray();

                metrics.CreateProjectFilesCount(onlyFiles.Length);

                foreach (var entry in onlyFiles)
                {
                    fileStream = new MemoryStream();

                    await entry.Open().CopyToAsync(fileStream);
                    var httpPath = $"/{entry.FullName.Replace('\\', '/')}";
                    byte[] siteItemData = fileStream.ToArray();
                    metrics.CreateProjectSiteItemUncompressedSize(siteItemData.Length);

                    // just to make 99.999% sure compression buffer will fit so even make compressed buffer exceed original size
                    // i dont belive that after compression size will be greated than 2 * orginal
                    // (but dont known brotli well, but what will happen if file has only two (2) bytes for example?)
                    // so choose arbitrary 4KB for minimium size after  compression (this value is just arbitrary, need to investigate what value should be min)
                    if (siteItemData.Length > compressBuffer.Length)
                    {
                        int extendedSize = siteItemData.Length < 4000 ? 4000 : 2 * siteItemData.Length;
                        compressBuffer = new byte[extendedSize];
                    }

                    if (BrotliEncoder.TryCompress(siteItemData, compressBuffer, out var compressedLength))
                    {
                        siteItemData = new byte[compressedLength];
                        Buffer.BlockCopy(compressBuffer, 0, siteItemData, 0, compressedLength);
                    }
                    else
                    {
                        logger.LogCritical("failed to compress file (brotli): {0} data to compress len: {1}", entry.FullName, siteItemData.Length);
                        throw new VHttpException(System.Net.HttpStatusCode.InternalServerError, $"failed to compress: {entry.FullName}");
                    }

                    metrics.CreateProjectSiteItemCompressedSize(compressedLength);

                    SiteItem si = new SiteItem(project.Id, httpPath, siteItemData);

                    // try to add something to shared_site_item
                    // or use exisintg if possible and already in db

                    //var candidateToSharedSiteItemsExtensions = new string[] {
                    //    ".css", ".js", ".woff", ".woff2",
                    //    ".min.js", ".min.js.map", ".map", ".svg", ".ico"
                    //};
                    string[] names = new string[] { "/favicon.ico", "/logo.svg" };
                    bool candidate = si.Path.StartsWith("/public", StringComparison.OrdinalIgnoreCase);
                    candidate |= names.Any(t => string.Compare(t, si.Path, StringComparison.OrdinalIgnoreCase) == 0);
                    if (candidate)
                    {
                        // just for lookups
                        var fullHash = SHA256.HashData(si.ByteData);
                        var sha256 = BitConverter.ToString(fullHash).Replace("-", "").ToLower();

                        // check maybe already exists
                        long? possibleMatch = await repository.SelectSharedSiteItemIdBySha256(sha256);

                        if (possibleMatch != null)
                        {
                            // so already exists, no need to save again same byte[] data in site_item
                            si.SharedSiteItemId = possibleMatch.Value;
                        }
                        else
                        {
                            // not exists, so adding this as new shared site item, maybe will be useful in future
                            SharedSiteItem newShared = new SharedSiteItem(si.Path, si.ByteData, sha256);

                            logger.LogInformation("adding new shared site item: Path: {0}, sha256: {1}, from project_id: {2} {3}",
                                newShared.Path, newShared.Sha256, projectId, projectName);

                            await repository.InsertSharedSiteItem(newShared);
                            si.SharedSiteItemId = newShared.Id;
                        }

                        si.ByteData = null;
                    }

                    await repository.InsertSiteHtmlAsync(si);
                }
            }

            metrics.CreateProjectEllapsedTime(sw.ElapsedMilliseconds);

            await repository.CommitAsync();
        }

        public async Task TryDeleteProjectByDnId(int dnProjectId, ProjectType type)
        {
            // is this safe to delete project?
            // maybe never delete and mark with 'deleted' flag?
            // not sure what to do
            logger.LogInformation("starting to delete project dn_id: {0}", dnProjectId);
            VValidate.EnumDefined(type);

            var project = await repository.SelectProjectByDnIdAsync(dnProjectId, type);

            if (project == null) logger.LogWarning($"trying delete project that does not exists: dnprojectid: '{dnProjectId}' type: '{type}'");
            else logger.LogInformation("delete project (project exists): dnid '{0}', type: '{1}'", dnProjectId, type);

            logger.LogTrace("Starting delete sitehtml, project_id: {0} dn_project_id: {1}", project.Id, project.DnProjectId);

            await repository.DeleteSiteHtmlByProjectIdAsync(project.Id);
            await repository.DeleteProjectAsync(project.Id);
            await repository.CommitAsync();

            logger.LogTrace("deleted project: dnid: {0} type: {1}", dnProjectId, type);
        }

        public void Ping()
        {
            throw new NotImplementedException();
        }
    }
}

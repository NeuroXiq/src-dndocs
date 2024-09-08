namespace DNDocs.Docs.Web.Services
{
    public class SqlText
    {
        public const string SelectSitemap = $"SELECT id as Id, [path] as Path, decompressed_length as DecompressedLength, " +
                "urls_count as UrlsCount, byte_data ByteData, updated_on as UpdatedOn " +
                "FROM sitemap";

        public const string SelectSitemap_NoData = $"SELECT id as Id, [path] as Path, decompressed_length as DecompressedLength, " +
                "urls_count as UrlsCount, NULL ByteData, updated_on as UpdatedOn " +
                "FROM sitemap";

        public const string SelectLastInsertRowId = "select last_insert_rowid()";
        public const string SelectMtMeasurement = "SELECT id as Id, [name] as Name, meter_name as MeterName, " +
                "instance_id as InstanceId, created_on as CreatedOn, tags as Tags, type as Type FROM mt_instrument";

        public const string UpdatePublicHtml =
@"UPDATE public_html SET [path] = @Path, byte_data = @ByteData, created_on = @CreatedOn, updated_on = @UpdatedOn";



        public const string SelectPublicHtml_NoData =
            @"SELECT id as Id, [path] as Path,      NULL as ByteData, created_on as CreatedOn, updated_on as UpdatedOn from public_html";

        public const string SelectPublicHtml =
            @"SELECT id as Id, [path] as Path, byte_data as ByteData, created_on as CreatedOn, updated_on as UpdatedOn from public_html";


        public const string SelectProject = @" SELECT
                    id AS Id, dn_project_id as DnProjectId, metadata as Metadata, url_prefix as UrlPrefix, project_version as ProjectVersion, 
                    nuget_package_name as NugetPackageName, nuget_package_version as NugetPackageVersion, project_type as ProjectType, 
                    created_on as CreatedOn, updated_on as UpdatedOn 
                    FROM project";

        public const string SelectSharedSiteItem =
@"SELECT 
id as Id, path as Path, byte_data as ByteData, sha_256 as Sha256
FROM shared_site_item
";

    }
}

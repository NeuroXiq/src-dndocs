using DNDocs.Docs.Web.Model;
using DNDocs.Docs.Web.Services;
using DNDocs.Docs.Web.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Collections.Immutable;
using System.IO.Compression;
using System.Text;
using Vinca.Http;
using Vinca.Http.Cache;

namespace DNDocs.Docs.Web.Web
{
    public class PublicContentController
    {
        public static ApiEndpoint[] GetEndpoints(IOptions<DOptions> settings)
        {
            var endpoints = new List<ApiEndpoint>()
            {
                GetEndpoint(HttpMethod.Get, "/n/{nugetPackageName}/{nugetPackageVersion}/{*slug}", GetNugetProjectSiteHtml),
                GetEndpoint(HttpMethod.Get, "/public/ping", Ping),
                GetEndpoint(HttpMethod.Get, "/system/projects/{pageNo?}", SystemAllProjects),
                GetEndpoint(HttpMethod.Get, "/system/site-items/{pageNo?}", SystemSiteItems),

                GetEndpoint(HttpMethod.Get, "{*slug}", GetPublicHtmlFile),
                GetEndpoint(HttpMethod.Get, "/sitemap.xml", GetSitemap),
                GetEndpoint(HttpMethod.Get, "/public/sitemaps/{*slug}", GetSitemap),
                new ApiEndpoint(HttpMethod.Get, $"/{settings.Value.IndexNowApiKey}.txt", GetIndexNowKey)
            };

            return endpoints.ToArray();
        }

        static ApiEndpoint GetEndpoint(HttpMethod method, string path, Delegate delegateMethod)
        {
            return new ApiEndpoint(method, path, delegateMethod);
        }

        static IResult Ping()
        {
            return Results.Ok();
        }

        static async Task<IResult> GetSitemap(
            HttpContext context,
            [FromServices] IQRepository qrepository)
        {
            string path = context.Request.Path.ToString();
            string contentType = "application/xml;charset=utf-8";
            context.Response.Headers.Append("Content-Encoding", "br");
            Sitemap sitemap = await qrepository.SelectSitemap(path);

            if (sitemap == null) return await NotFound();

            return Results.File(sitemap.ByteData, contentType);
        }

        static IResult GetIndexNowKey([FromServices] IOptions<DOptions> settingsOptions)
        {
            var dsettings = settingsOptions.Value;
            string indexNowKey = dsettings.IndexNowApiKey;

            return Results.File(Encoding.UTF8.GetBytes(indexNowKey), "text/plain");
        }

        static async Task<IResult> GetPublicHtmlFile(
            HttpContext context,
            [FromServices] IDMemCache memCache,
            string slug)
        {
            if (string.IsNullOrWhiteSpace(slug) || slug == "/") slug = "index.html";

            PublicHtml publicHtml = await memCache.GetPublicHtmlFile($"/{slug}");
            if (publicHtml == null) return await NotFound();

            string contentType = null;
            byte[] byteData = publicHtml.ByteData;

            contentType = HttpContentTypeMaps.GetFromPathOrFallback(publicHtml.Path);

            return Results.File(byteData, contentType);
        }

        [VCacheControl(CacheType = CacheControlType.Public, MaxAge = 30 * 60)]
        static async Task<IResult> GetNugetProjectSiteHtml(
            HttpContext context,
            [FromServices] IOptions<DOptions> dsettings,
            [FromServices] IDMemCache memCache,
            [FromServices] IQRepository repository,
            [FromRoute] string nugetPackageName,
            [FromRoute] string nugetPackageVersion,
            string slug)
        {
            // todo when case invalid (e.g. /AUTOmapper instead of /AutoMapper) do redirect to valid case (must include {*slug});
            //      should be case sensitive
            Project project = await memCache.GetNugetProject(nugetPackageName, nugetPackageVersion);

            if (project == null) return Results.Redirect(dsettings.Value.GetUrlNugetProjectGenerate(nugetPackageName, nugetPackageVersion));

            return await ReturnSiteItem(context, memCache, ProjectType.Nuget, slug, nugetPackageName, nugetPackageVersion, null, null);
        }

        private static async Task<IResult> NotFound()
        {
            return Results.NotFound();
        }

        private static async Task<IResult> ReturnSiteItem(
            HttpContext context,
            IDMemCache memCache,
            ProjectType projectType,
            string slug,
            string nugetPackageName,
            string nugetPackageVersion,
            string urlPrefix,
            string versionTag
            )
        {
            var path = $"/{slug}";
            Project project = null;
            
            switch (projectType)
            {
                case ProjectType.Nuget: project = await memCache.GetNugetProject(nugetPackageName, nugetPackageVersion); break;
                default: throw new NotImplementedException();
            }

            if (project == null) return Results.NotFound();
            if (path == "/favicon.ico") return await GetPublicHtmlFile(context, memCache, "favicon.ico");

            var siteItem = await memCache.GetSiteItem(project.Id, path);

            if (siteItem == null) return Results.NotFound();

            byte[] byteData = siteItem.ByteData;

            if (!context.Request.Headers.AcceptEncoding.Any(t => t.Contains("br")))
            {
                // should be very very rare case but to be 100% sure support decompress
                // (bytes in db are brotli compressed)
                var compressed = new MemoryStream(byteData);
                var decompressed = new MemoryStream();

                using BrotliStream brotliStream = new BrotliStream(compressed, CompressionMode.Decompress);
                brotliStream.CopyTo(decompressed);
                brotliStream.Flush();
                brotliStream.Close();

                byteData = decompressed.ToArray();
            }
            else
            {
                context.Response.Headers.Append("Content-Encoding", "br");
            }

            return Results.File(byteData, HttpContentTypeMaps.GetFromPathOrFallback(siteItem.Path));
        }

        #region System pages


        public static async Task<IResult> SystemSiteItems(
            [FromServices] IOptions<DOptions> settings,
            [FromServices] IQRepository repository,
            [FromRoute] int? pageNo)
        {
            pageNo = pageNo ?? 0;
            var siteitems = await repository.GetSiteItemPagedAsync(pageNo.Value, 1000);
            var siteItemsCount = await repository.SelectSiteItemCount();
            var allprojs = await repository.SelectProjectPagedAsync(0, 100000);

            var pdics = allprojs.ToImmutableDictionary(x => x.Id);

            var sb = new StringBuilder();
            AppendHtmlTable(sb,
                new[] { "ProjectId", "ProjectName", "FullUrl" },
                new Func<SiteItem, object>[]
                {
                    s => pdics.TryGetValue(s.ProjectId, out var value) ? value?.ToString() : "<null>",
                    s => pdics.TryGetValue(s.ProjectId, out var value) ? $"{value.NugetPackageName} {value.NugetPackageVersion}" : "<null>",
                    s =>
                    {
                        var u = pdics.TryGetValue(s.ProjectId, out var value) ?
                            FullProjectUrl(settings.Value, value, s.Path) : "<null>";

                        return $"<a href=\"{u}\">{u}</a>";
                    }
                },
                siteitems);

            AppendSimpleHtmlPagination(sb, (long)Math.Ceiling((double)siteItemsCount / 1000), "/system/site-items/{0}");
            SimpleHtmlPage(sb);

            var result = sb.ToString();
            return Results.Content(result, "text/html");
        }


        public static async Task<IResult> SystemAllProjects(
            [FromServices] IOptions<DOptions> settings,
            [FromServices] IQRepository repository,
            [FromRoute] int? pageNo)
        {
            var projects = await repository.SelectProjectPagedAsync(pageNo ?? 0, 1000);
            if (projects.Length == 0) return Results.Content("no projects", "text/html");

            var sb = new StringBuilder();

            AppendHtmlTable(sb, new[] { "id", "dn id", "type", "packagename", "packagever", "urlprefix", "dn-url-generate", "ddocs-url" },
            new Func<Project, string>[]
            {
                (Project p) => p.Id.ToString(),
                (Project p) => p.DnProjectId.ToString(),
                (Project p) => p.ProjectType.ToString(),
                (Project p) => p.NugetPackageName ?? "",
                (Project p) => p.NugetPackageVersion ?? "",
                (Project p) => p.UrlPrefix ?? "",
                (Project p) => { var u = settings.Value.GetUrlNugetProjectGenerate(p.NugetPackageName, p.NugetPackageVersion);  return $"<a href=\"{u}\">{u}</a>"; },
                (Project p) => { var u = FullProjectUrl(settings.Value, p); return $"<a href=\"{u}\">{u}</a>"; }
            }, projects);

            SimpleHtmlPage(sb);


            return Results.Content(sb.ToString(), "text/html");

            //var td = 

            //int[] tabs = new int[td.Length];

            //for (int i = 0; i < td.Length; i++)
            //{
            //    tabs[i] = projects.Max(p => (int)Math.Ceiling((double)td[i](p).Length / 4));
            //}

            //sb.Append("<head></head><body>");
            //sb.Append("<table>");
            //foreach (var p in projects)
            //{
            //    // sb.Append("|");
            //    sb.Append("<tr>");
            //    for (int i = 0; i < td.Length; i++)
            //    {
            //        var value = td[i](p);
            //        // var spaces = 4 * tabs[i] - value.Length;
            //        // sb.Append($" {value}{new string(' ', spaces)} |");
            //        if (i != td.Length - 1) { sb.Append("<td>"); sb.Append(value); sb.Append("</td>"); }
            //        else { sb.Append("<td>"); sb.Append($"<a href=\"{value}\">{value}</a>"); sb.Append("</td>"); }
            //    }
            //    sb.Append("<tr>");



            //    // sb.AppendLine();
            //}
            //sb.Append("<table>");
            //sb.Append("</body>");

        }

        public static string FullProjectUrl(DOptions s, Project p, string path = "/api/index.html")
        {
            return s.GetUrlNugetOrgProject(p.NugetPackageName, p.NugetPackageVersion, path);
        }

        #endregion

        private static void AppendSimpleHtmlPagination(StringBuilder b, long pagesCount, string formatUrl)
        {
            b.Append("<div>Pages</div>");
            b.Append("<div>");
            var hrefFormat = $"<a style=\"margin: 0 8px;\"href=\"{formatUrl}\">{{0}}</a>";
            for (int i = 0; i < pagesCount; i++) b.AppendFormat(hrefFormat, i);
            b.Append("<div>");
        }

        public static IResult SimpleHtmlPage(StringBuilder body, string headTags = null)
        {
            StringBuilder sb = new StringBuilder();
            string headFormat =
            """
<html>
 <head>
 {0}

 </head>
""";
            string bodyFormat =
@"
<body>
 <style>
 table, th, td {{
 border: 1px solid black;
 }}
td {{
padding: 8 12px;
}}
 table {{
 border-collapse: collapse;
 }}
 </style>

 {0}
 </body>
<html>
";
            sb.AppendFormat(headFormat, headTags ?? "");
            sb.AppendFormat(bodyFormat, body);

            return Results.Content(sb.ToString(), "text/html");
        }

        public static void AppendHtmlTable<T>(StringBuilder sb, string[] columns, Func<T, object>[] config, IEnumerable<T> values)
        {
            if (columns.Length != config.Length) throw new ArgumentException("config.lenth != columns.length");

            sb.Append("<table>");
            sb.Append("<thead><tr>");

            foreach (var item in columns)
            {
                sb.AppendFormat("<td>{0}</td>", item);
            }

            sb.Append("</tr></thead><tbody>");

            foreach (var item in values)
            {
                sb.Append("<tr>");
                for (int i = 0; i < columns.Length; i++)
                {
                    sb.AppendFormat("<td>{0}</td>", config[i](item));
                }
                sb.Append("</tr>");
            }

            sb.Append("</tbody></table>");
        }
    }
}
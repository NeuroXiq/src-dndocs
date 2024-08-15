using DNDocs.Docs.Web.Model;
using DNDocs.Docs.Web.Services;
using DNDocs.Docs.Web.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Diagnostics.ResourceMonitoring;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Text;
using Vinca.Exceptions;
using Vinca.Http;
using Vinca.Http.Cache;

namespace DNDocs.Docs.Web.Web
{
    public class PublicContentController
    {
        public static readonly ApiEndpoint[] Endpoints = new ApiEndpoint[]
        {
            GetEndpoint(HttpMethod.Get, "/n/{nugetPackageName}/{nugetPackageVersion}/{*slug}", GetNugetProjectSiteHtml),
            GetEndpoint(HttpMethod.Get, "/v/{urlPrefix}/{versionTag}/{*slug}", GetVersionProjectSiteHtml),
            GetEndpoint(HttpMethod.Get, "/s/{urlPrefix}/{*slug}", GetSingletonProjectSiteHtml),
            GetEndpoint(HttpMethod.Get, "/public/ping", Ping),
            GetEndpoint(HttpMethod.Get, "/system/projects/{pageNo?}", SystemAllProjects),
            GetEndpoint(HttpMethod.Get, "/system/site-items/{pageNo?}", SystemSiteItems),
            GetEndpoint(HttpMethod.Get, "/system/resource-monitor", SystemResourceMonitoring),

            GetEndpoint(HttpMethod.Get, "/{*slug}", GetPublicHtmlFile),
        };

        static ApiEndpoint GetEndpoint(HttpMethod method, string path, Delegate delegateMethod)
        {
            return new ApiEndpoint(method, path, delegateMethod);
        }

        static IResult Ping()
        {
            return Results.Ok();
        }

        static async Task<IResult> GetPublicHtmlFile(
            HttpContext context,
            [FromServices] IDMemCache memCache,
            string slug)
        {
            PublicHtml publicHtml = await memCache.GetPublicHtmlFile($"/{slug}");
            if (publicHtml == null) return await NotFound();

            string contentType = null;
            byte[] byteData = publicHtml.ByteData;

            if (slug == "sitemap.xml" || slug.StartsWith("sitemaps/"))
            {
                contentType = "application/xml;charset=utf-8";
                context.Response.Headers.Append("Content-Encoding", "br");
                // byte[] decompressed = new byte[10000000];
                // BrotliDecoder.TryDecompress(byteData, decompressed, out var dlen);
                // byteData = new byte[dlen];
                // Buffer.BlockCopy(decompressed, 0, byteData, 0, dlen);
            }
            else contentType = HttpContentTypeMaps.GetFromPathOrFallback(publicHtml.Path);

            return Results.File(byteData, contentType);
        }

        static async Task<IResult> GetSingletonProjectSiteHtml(
            HttpContext context,
            [FromServices] IDMemCache memCache,
            [FromRoute] string urlPrefix, string slug)
        {
            return await ReturnSiteItem(context, memCache, ProjectType.Singleton, slug, null, null, urlPrefix, null);
        }

        static async Task<IResult> GetVersionProjectSiteHtml([FromRoute] string urlPrefix, [FromRoute] string versionTag, string slug)
        {
            return Results.Content("not implemented", "text/plain");
        }

        [VCacheControl(CacheType = CacheControlType.Public, MaxAge = 30 * 60)]
        static async Task<IResult> GetNugetProjectSiteHtml(
            HttpContext context,
            [FromServices] IOptions<DSettings> dsettings,
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
                case ProjectType.Singleton: await memCache.GetSingletonProject(urlPrefix); break;
                case ProjectType.Version: throw new NotImplementedException(); break;
                case ProjectType.Nuget: project = await memCache.GetNugetProject(nugetPackageName, nugetPackageVersion);  break;
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

                var compressed = new MemoryStream();
                var decompressed = new MemoryStream();

                compressed.Write(byteData);
                BrotliStream brotliStream = new BrotliStream(compressed, CompressionMode.Decompress);
                brotliStream.CopyTo(decompressed);

                byteData = decompressed.ToArray();
            }
            else
            {
                context.Response.Headers.Append("Content-Encoding", "br");
            }

            return Results.File(byteData, HttpContentTypeMaps.GetFromPathOrFallback(siteItem.Path));
        }

        #region System pages

        public static async Task<IResult> SystemResourceMonitoring([FromServices] IQRepository repository)
        {
            IEnumerable<ResourceMonitorUtilization> utilization = await repository.SelectResourceMonitorUtilization(50);
            var sb = new StringBuilder();
            AppendHtmlTable(sb,
                new string[] { "id", "date_time", "cpu_used_percentage", "memory_userd_in_bytes", "memory_used_percentage" },
                new Func<ResourceMonitorUtilization, object>[]
                {
                    c => c.Id,
                    c => c.DateTime.ToString("yyyy-MM-dd T HH:mm:ss"),
                    c => Math.Round(c.CpuUsedPercentage, 2) + "%",
                    c => Math.Round(c.MemoryUsedInBytes/(double)1000000) + "MB",
                    c => Math.Round(c.MemoryUsedPercentage, 2) + "%"
                },
                utilization);

            int refreshRate = 5;

#if DEBUG
            refreshRate = 1;
#endif

            return SimpleHtmlPage(sb, $"<meta http-equiv=\"refresh\" content=\"{refreshRate}\">");
        }

        //public static async Task<IResult> SystemMetrics(
        //    [FromServices] IQRepository repository)
        //{
        //    var s = await repository.SelectSystemStats();

        //    StringBuilder sbt = new StringBuilder();
          
        //    return SimpleHtmlPage("", sbt);

        //    // var sb = new StringBuilder();
        //    // sb.AppendFormat("SiteItemCount: {0}", s.SiteItemCount);
        //    // sb.AppendLine();
        //    // sb.AppendFormat("SharedSiteItemCount: {0}", s.SharedSiteItemCount);
        //    //sb.AppendLine();
        //    //var sharedUsagePercent = s.SiteItemCountUsingShared == 0 ?
        //    //    "0" :
        //    //    Math.Round((double)100 * s.SiteItemCountUsingShared / s.SiteItemCount, 2).ToString();

        //    //sb.AppendFormat("SiteItemCountUsingShared: {0} ({1}%)", s.SharedSiteItemCount, sharedUsagePercent);
        //    //sb.AppendLine();
        //    //sb.AppendFormat("AppLogCount: {0}", s.AppLogCount);
        //    //sb.AppendLine();
        //    //sb.AppendFormat("HttpLogCount: {0}", s.HttpLogCount);
        //    //sb.AppendLine();
        //    //sb.AppendFormat("ProjectCount: {0}", s.ProjectCount);
        //    //sb.AppendLine();
        //}

        public static async Task<IResult> SystemSiteItems(
            [FromServices] IOptions<DSettings> settings,
            [FromServices] IQRepository repository,
            [FromRoute] int? pageNo)
        {
            var siteitems = await repository.GetSiteItemPagedAsync(0, 1000);
            var allprojs = await repository.SelectProjectPagedAsync(0, 100);
            var pdics = allprojs.ToImmutableDictionary(x => x.Id);

            var sb = new StringBuilder();
            sb.Append("<head></head><body><table>");
            sb.Append("<thead><tr> <td>SiteItemId</td>  <td>ProjectID</td> <td>Url</td> </tr></thead>");
            foreach (var si in siteitems)
            {
                sb.Append("<tr>");
                sb.AppendFormat("<td>{0}</td>", si.Id);
                sb.AppendFormat("<td>{0}</td>", si.ProjectId);
                sb.Append("<td>");
                sb.AppendFormat("<a href=\"{0}\">{0}</a>", FullProjectUrl(settings.Value, pdics[si.ProjectId], si.Path));
                sb.Append("</td>");
                sb.Append("</tr>");
            }
            sb.Append("</table></body>");

            var result = sb.ToString();
            return Results.Content(result, "text/html");
        }

        public static async Task<IResult> SystemAllProjects(
            [FromServices] IOptions<DSettings> settings,
            [FromServices] IQRepository repository,
            [FromRoute] int? pageNo)
        {
            var projects = await repository.SelectProjectPagedAsync(pageNo ?? 0, 1000);
            if (projects.Length == 0) return Results.Content("no projects", "text/html");

            var sb = new StringBuilder();
            
            AppendHtmlTable(sb, new[] { "id", "dn id", "type", "packagename", "packagever", "urlprefix", "version", "fullurl" },
            new Func<Project, string>[]
            {
                (Project p) => p.Id.ToString(),
                (Project p) => p.DnProjectId.ToString(),
                (Project p) => p.ProjectType.ToString(),
                (Project p) => p.NugetPackageName ?? "",
                (Project p) => p.NugetPackageVersion ?? "",
                (Project p) => p.UrlPrefix ?? "",
                (Project p) => p.ProjectVersion ?? "",
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


        static string FullProjectUrl(DSettings s, Project p, string path = "/api/index.html")
        {
            if (p.ProjectType == ProjectType.Nuget) return s.GetUrlNugetOrgProject(p.NugetPackageName, p.NugetPackageVersion, path);
            else if (p.ProjectType == ProjectType.Singleton) return s.GetUrlSingletonProject(p.UrlPrefix, path);
            else return s.GetUrlVersionProject(p.UrlPrefix, p.ProjectVersion, path);
        }

        #endregion

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
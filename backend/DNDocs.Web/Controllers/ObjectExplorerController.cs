using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using DNDocs.Application.Shared;
using System.Text;
using System.Web;
using Microsoft.Extensions.Options;
using DNDocs.Shared.Configuration;

namespace DNDocs.Web.Controllers
{
    public class ObjectExplorerController : ApiControllerBase
    {
        private static readonly FileExtensionContentTypeProvider fileExtensionContentTypeProvider = new FileExtensionContentTypeProvider();
        private IQueryDispatcher qd;

        public ObjectExplorerController(IQueryDispatcher qd)
        {
            this.qd = qd;
        }

        [HttpGet("/d/cookies-consent.html")]
        [ResponseCache(Duration = 36000)]
        public IActionResult CookiesConsentHtml()
        {
            return Content(DNDocs.Resources.AppResources.CookiesConsentHtmlContent, "application/text");
        }

        [HttpGet("/d/dndocs-docfx-script.js")]
        [ResponseCache(Duration = 36000)]
        public IActionResult DndocsGeneratedWebsiteInject()
        {
            return Content(DNDocs.Resources.AppResources.DNDocsDocfxScriptJsContent, "application/javascript");
        }

        [HttpGet("/d/{projectUrlPrefix}/{*slug}")]
        [ResponseCache(Duration = 36000)]
        public IActionResult Docfx([FromRoute] string projectUrlPrefix, [FromRoute] string slug)
        {
            // todo: remove this, later fix invalid links if neede
            string basePath = "https://dndocs.com/?packageName={0}&packageVersion={1}&r=" + Guid.NewGuid(); // for avoid caching
            //string basePath = "http://localhost:3000/?packageName={0}&packageVersion={1}";
            string redirect = "";

            if (projectUrlPrefix == "betalgo-openai" && slug.Contains("Utilities")) redirect = string.Format(basePath, "Betalgo.OpenAI", "8.6.1");
            else if (projectUrlPrefix == "betalgo-openai") redirect = string.Format(basePath, "Betalgo.OpenAI.Utilities", "8.0.1");
            else if (projectUrlPrefix == "distributedlock")
            {
                if (slug.Contains("SqlServer")) redirect = string.Format(basePath, "DistributedLock.SqlServer", "1.0.5");
                else if (slug.Contains("Postgres")) redirect = string.Format(basePath, "DistributedLock.Postgres", "1.2.0");
                else if (slug.Contains("MySql")) redirect = string.Format(basePath, "DistributedLock.MySql", "1.0.2");
                else if (slug.Contains("Oracle")) redirect = string.Format(basePath, "DistributedLock.Oracle", "1.0.3");
                else if (slug.Contains("Redis")) redirect = string.Format(basePath, "DistributedLock.Redis", "1.0.3");
                else if (slug.Contains("Azure")) redirect = string.Format(basePath, "DistributedLock.Azure", "1.0.1");
                else if (slug.Contains("ZooKeeper")) redirect = string.Format(basePath, "DistributedLock.ZooKeeper", "1.0.0");
                else if (slug.Contains("FileSystem")) redirect = string.Format(basePath, "DistributedLock.FileSystem", "1.0.2");
                else if (slug.Contains("WaitHandles")) redirect = string.Format(basePath, "DistributedLock.WaitHandles", "1.0.1");
            }
            else if (projectUrlPrefix == "sharpcompress") redirect = string.Format(basePath, "SharpCompress", "0.37.2");

            if (redirect != "") return Redirect(redirect);

            return Redirect($"https://docs.dndocs.com/s/{projectUrlPrefix}/{slug}");

            //string currentTenant = projectUrlPrefix;
            //if (string.IsNullOrEmpty(slug) || slug == "/") return Redirect($"/d/{currentTenant}/index.html");
            //else slug = "/" + slug;

            //// maybe implement cache in RAM here for all files (will invoke soon next requets)
            //// maybe do not call query handler and get data directly here in this method from repo
            //// is query handler needed?

            //var siteitem = qd.DispatchSync(new GetDocfxSiteItemQuery(projectUrlPrefix, slug)).Result;

            //// if (siteitem == null) return base.NotFoundView("DocfxSiteItem", new[] { "path", (slug ?? "") });

            //if (siteitem == null)
            //{
            //    string info = "PATH: " + slug ?? "";
            //    info += "\r\nProject Url prefix: " + (projectUrlPrefix ?? "");

            //    return Content(ViewNotFound(info), "text/html; charset=UTF-8");
            //}
            //else if (!slug.EndsWith(".html"))
            //{
            //    const string DefaultContentType = "application/octet-stream";

            //    if (!fileExtensionContentTypeProvider.TryGetContentType(slug, out string contentType))
            //    {
            //        contentType = DefaultContentType;
            //    }

            //    return File(siteitem.Content, contentType);
            //}

            //return Content(Encoding.UTF8.GetString(siteitem.Content), "text/html; charset=UTF-8");

            // is this needed?
            // var vm = new DocfxViewModel(siteitem);
            // 
            // return View(vm);
        }

        private string ViewNotFound(string info)
        {
            string content = $@"
<head>
<link rel=""stylesheet"" href=""/api/css/apicss.css"">
</head>
<body>
<div class=""status-code"">
<h1>
404 - Not Found
</h1>
<h2>
Requested resource was not found
</h2>
<pre>{HttpUtility.HtmlEncode(info ?? "")}</pre>
<hr />
<h2><a href=""https://www.robiniadocs.com/"">https://www.robiniadocs.com/</a></h2>
</div>
</body>
";
            return content;
        }
    }
}

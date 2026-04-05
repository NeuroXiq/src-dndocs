using Microsoft.AspNetCore.Mvc;
using Vinca.Utils;

namespace DNDocs.App.Web.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet("/api/system/health")]
        public IActionResult Health()
        {
            return Json(SystemHealthApiResponse.Create(typeof(DNDocs.Web.Program).Assembly));
        }

        public class GenerateModel
        {
            public string PackageName { get; set; }
            public string PackageVersion { get; set; }
        }

        [HttpGet("/generate/{packageName}/{packageVersion}")]
        public IActionResult Generate(string packageName, string packageVersion)
        {
            var model = new GenerateModel() { PackageName = packageName, PackageVersion = packageVersion };
            return View("Index", model);
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}

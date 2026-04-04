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

        public IActionResult Index()
        {
            return View();
        }
    }
}

using Microsoft.AspNetCore.Mvc;

namespace DNDocs.App.Web.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet("/system/health")]
        public IActionResult Health()
        {
            return Json(new { Online = true, AppName = "DNDocs.App", Timestamp = DateTime.UtcNow });
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}

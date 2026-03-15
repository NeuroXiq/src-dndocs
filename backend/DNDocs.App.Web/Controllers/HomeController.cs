using Microsoft.AspNetCore.Mvc;

namespace DNDocs.App.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

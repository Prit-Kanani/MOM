using Microsoft.AspNetCore.Mvc;

namespace MoM.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult Editor(int? id)
        {
            ViewBag.MeetingId = id;
            return View();
        }

        public IActionResult Preview(int id)
        {
            ViewBag.MeetingId = id;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}

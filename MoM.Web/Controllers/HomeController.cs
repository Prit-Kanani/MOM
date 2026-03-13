using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MoM.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult MeetingVault()
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
        [AllowAnonymous]
        public IActionResult Error()
        {
            return View();
        }
    }
}

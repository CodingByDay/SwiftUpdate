using Microsoft.AspNetCore.Mvc;

namespace SwiftUpdate.Controllers
{
    public class LandingController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

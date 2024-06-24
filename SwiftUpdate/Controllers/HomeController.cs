using Microsoft.AspNetCore.Mvc;
using SwiftUpdate.Models;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;

namespace SwiftUpdate.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SwiftUpdateContext _context;

        public HomeController(SwiftUpdateContext context, ILogger<HomeController> logger)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        public IActionResult Dashboard()
        {
            List<ApplicationModel> applications = _context.Applications.ToList();
            return View(applications);
        }
        // POST: Create a new application
        [HttpPost]
        public async Task<IActionResult> Create(ApplicationModel model)
        {
            if (ModelState.IsValid)
            {
                // Assuming you have a data access layer or repository to save to the database
                bool success = await SaveApplicationToDatabase(model);

                if (success)
                {
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, errorMessage = "Failed to save application." });
                }
            }
            else
            {
                return Json(new { success = false, errorMessage = "Invalid model state." });
            }
        }

        // Example method to save application to the database (replace with your actual implementation)
        private async Task<bool> SaveApplicationToDatabase(ApplicationModel model)
        {
            try
            {
                _context.Applications.Add(model);
                await _context.SaveChangesAsync();

                // For demonstration purposes, returning true here
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                return false;
            }
        }


    }
}

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
        private readonly IWebHostEnvironment _env;

        public HomeController(SwiftUpdateContext context, ILogger<HomeController> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _context = context;
            _env = env;

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

                    // Get the path to the ApplicationData folder within the app's directory
                    string appDataFolderPath = Path.Combine(_env.ContentRootPath, "ApplicationData", model.ApplicationName);

                    // Check if the folder exists
                    if (!Directory.Exists(appDataFolderPath))
                    {
                        // Create the folder if it doesn't exist
                        Directory.CreateDirectory(appDataFolderPath);
                        return Json(new { success = true });

                    }
                    else
                    {
                        return Json(new { success = true });

                    }


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


        [HttpPost]
        public async Task<IActionResult> Delete(ApplicationModel model)
        {
            try
            {
                var CompleteModel = await _context.Applications.FindAsync(model.ApplicationId);
                
                if(CompleteModel == null)
                {
                    return Json(new { success = false, errorMessage = "Failed to delete application." });
                }

                // Assuming you have a data access layer or repository to delete from the database
                bool success = await DeleteApplicationFromDatabaseAsync(CompleteModel.ApplicationId);
                if (success)
                {
                    // Get the path to the ApplicationData folder within the app's directory
                    string appDataFolderPath = Path.Combine(_env.ContentRootPath, "ApplicationData", CompleteModel?.ApplicationName ?? string.Empty);

                    // Check if the folder exists
                    if (Directory.Exists(appDataFolderPath))
                    {
                        // Create the folder if it doesn't exist
                        Directory.Delete(appDataFolderPath);
                        return Json(new { success = true });

                    }
                    else
                    {
                        return Json(new { success = true });

                    }
                }
                else
                {
                    return Json(new { success = false, errorMessage = "Failed to delete application." });
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                return Json(new { success = false, errorMessage = "Failed to delete application." });
            }
        }

        // Example async method to delete application from the database (replace with your actual implementation)
        private async Task<bool> DeleteApplicationFromDatabaseAsync(int id)
        {
            try
            {
                var application = await _context.Applications.FindAsync(id);
                if (application != null)
                {
                    _context.Applications.Remove(application);
                    await _context.SaveChangesAsync();
                    return true;
                }
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

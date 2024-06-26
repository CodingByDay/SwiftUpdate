using Microsoft.AspNetCore.Mvc;
using SwiftUpdate.Helpers;
using SwiftUpdate.Models;
using SwiftUpdate.Services;
using SwiftUpdate.ViewModels;
using System.Diagnostics;
using System.IO.Compression;
using System.Xml;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace SwiftUpdate.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SwiftUpdateContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly SessionService _sessionService;

        public HomeController(SwiftUpdateContext context, ILogger<HomeController> logger, IWebHostEnvironment env, SessionService sessionService)
        {
            _logger = logger;
            _context = context;
            _env = env;
            _sessionService = sessionService;
        }

        public IActionResult Index()
        {
            // Get session information
            var sessionGuid = HttpContext.Request.Cookies["SessionGuid"]; 

            // Example: Retrieve session data from service
            var sessionData = _sessionService.GetSessionByGuid(sessionGuid); // Implement this method in your service

            // Pass session data to ViewData or ViewBag
            if (sessionData != null)
            {
                ViewData["SessionData"] = sessionData;
                return RedirectToAction("Dashboard", "Home");

            } else
            {
                return RedirectToAction("Login", "Account");

            }

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
            // Get session information
            var sessionGuid = HttpContext.Request.Cookies["SessionGuid"]; 

            // Example: Retrieve session data from service
            var sessionData = _sessionService.GetSessionByGuid(sessionGuid ?? string.Empty); 

            // Pass session data to ViewData or ViewBag
            if (sessionData == null)
            {
                ViewData["SessionData"] = string.Empty;
                return RedirectToAction("Login", "Account");
            } else
            {
                ViewData["SessionData"] = sessionData;
            }
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
        public async Task<IActionResult> Versions(int id)
        {
            // Use the 'id' parameter for your logic
            // For example, you might retrieve versions based on the id
            var applicationModel = await _context.Applications.FindAsync(id);

            // Ensure the application model exists
            if (applicationModel == null)
            {
                return NotFound();
            }

            // Construct the folder path based on application name
            string appDataFolderPath = Path.Combine(_env.ContentRootPath, "ApplicationData", applicationModel.ApplicationName);

            var viewModel = Methods.FindAndReturnModelVersions(appDataFolderPath, applicationModel);

            if (viewModel != null)
            {
                return View(viewModel);
            }
            else
            {
                return NotFound();
            }
        }

        private int? ExtractAndConvertVersionCode(string fileName)
        {
            // Example: Extract version code from file name following '__v'
            // Convert to int and return the highest number
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            var startIndex = fileNameWithoutExtension.LastIndexOf("__v", StringComparison.OrdinalIgnoreCase);
            if (startIndex != -1 && startIndex + 3 < fileNameWithoutExtension.Length)
            {
                var versionString = fileNameWithoutExtension.Substring(startIndex + 3);
                if (int.TryParse(versionString, out int versionCode))
                {
                    return versionCode;
                }
            }

            return null; // Return null if version code pattern not found or cannot be parsed
        }



        private readonly long _fileSizeLimit = 500L * 1024 * 1024; // 500 MB

        [HttpPost]
        public async Task<IActionResult> UploadApk(IFormFile apkFile, int applicationId)
        {
            try
            {
                var applicationModel = await _context.Applications.FindAsync(applicationId);



                if (apkFile == null || apkFile.Length == 0)
                {
                    ViewBag.Message = "No file selected.";
                    return View("Versions");
                }

                if (apkFile.Length > _fileSizeLimit)
                {
                    ViewBag.Message = "File size exceeds 500 MB limit.";
                    return View("Versions");
                }

                var uploads = Path.Combine(_env.ContentRootPath, "ApplicationData", applicationModel?.ApplicationName ?? string.Empty);

                if (!Directory.Exists(uploads))
                {
                    Directory.CreateDirectory(uploads);
                }

                var filePath = Path.Combine(uploads, apkFile.FileName);

                try
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await apkFile.CopyToAsync(stream);
                    }

                    ViewBag.Message = "File uploaded successfully.";
                }
                catch (Exception ex)
                {
                    ViewBag.Message = $"File upload failed: {ex.Message}";
                }


                var viewModel = Methods.FindAndReturnModelVersions(uploads, applicationModel);

                return View("Versions", viewModel);
            } catch (Exception ex)
            {
                return View("Dashboard");
            }
        }

    }
}

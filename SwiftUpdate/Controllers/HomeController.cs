using Microsoft.AspNetCore.Mvc;
using SwiftUpdate.Helpers;
using SwiftUpdate.Models;
using SwiftUpdate.Services;
using SwiftUpdate.ViewModels;
using System.Diagnostics;
using System.IO.Compression;
using System.Text.RegularExpressions;
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

            try
            {
                // Get session information
                var sessionGuid = HttpContext.Request.Cookies["SessionGuid"];
                // Example: Retrieve session data from service
                var sessionData = _sessionService.GetSessionByGuid(sessionGuid); // Implement this method in your service

                // Pass session data to ViewData or ViewBag
                if (sessionData != null && sessionData?.ExpiryTime > DateTime.Now)
                {
                    ViewData["SessionData"] = sessionData;
                    return RedirectToAction("Dashboard", "Home");

                }
                else
                {
                    return RedirectToAction("Login", "Account");

                }
            } catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
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
            try
            {
                // Get session information
                var sessionGuid = HttpContext.Request.Cookies["SessionGuid"];

                // Example: Retrieve session data from service
                var sessionData = _sessionService.GetSessionByGuid(sessionGuid ?? string.Empty);

                if (sessionData != null && sessionData?.ExpiryTime > DateTime.Now)
                {
                    ViewData["SessionData"] = sessionData;
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
                List<ApplicationModel> applications = _context.Applications.ToList();
                return View(applications);
            } catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                throw;
            }
        }
        // POST: Create a new application
        [HttpPost]
        public async Task<IActionResult> Create(ApplicationModel model)
        {
            try
            {
                var sessionGuid = HttpContext.Request.Cookies["SessionGuid"];

                // Example: Retrieve session data from service
                var sessionData = _sessionService.GetSessionByGuid(sessionGuid ?? string.Empty);


                if (sessionData != null && sessionData?.ExpiryTime > DateTime.Now)
                {
                    ViewData["SessionData"] = sessionData;
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }


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
            } catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                throw;
            }
        }

        // Example method to save application to the database 
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
                SentrySdk.CaptureException(ex);
                return false;
            }
        }


        [HttpPost]
        public async Task<IActionResult> Delete(ApplicationModel model)
        {
            try
            {
                var sessionGuid = HttpContext.Request.Cookies["SessionGuid"];

                var sessionData = _sessionService.GetSessionByGuid(sessionGuid ?? string.Empty);

                if (sessionData != null && sessionData?.ExpiryTime > DateTime.Now)
                {
                    ViewData["SessionData"] = sessionData;
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }


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
                SentrySdk.CaptureException(ex);
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
                SentrySdk.CaptureException(ex);
                return false;
            }
        }
        public async Task<IActionResult> Versions(int id)
        {
            try
            {
                var sessionGuid = HttpContext.Request.Cookies["SessionGuid"];

                // Example: Retrieve session data from service
                var sessionData = _sessionService.GetSessionByGuid(sessionGuid ?? string.Empty);

                // Pass session data to ViewData or ViewBag
                if (sessionData != null && sessionData?.ExpiryTime > DateTime.Now)
                {
                    ViewData["SessionData"] = sessionData;
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
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
            } catch(Exception ex)
            {
                SentrySdk.CaptureException(ex);
                throw;
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

            var applicationModel = await _context.Applications.FindAsync(applicationId);
            var uploads = Path.Combine(_env.ContentRootPath, "ApplicationData", applicationModel?.ApplicationName ?? string.Empty);
            var viewModel = Methods.FindAndReturnModelVersions(uploads, applicationModel);
            try
            {
                var sessionGuid = HttpContext.Request.Cookies["SessionGuid"];

                // Example: Retrieve session data from service
                var sessionData = _sessionService.GetSessionByGuid(sessionGuid ?? string.Empty);

                // Pass session data to ViewData or ViewBag
                if (sessionData != null && sessionData?.ExpiryTime > DateTime.Now)
                {
                    ViewData["SessionData"] = sessionData;
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }

                if (apkFile == null || apkFile.Length == 0)
                {
                    ViewBag.Error = "No file selected.";

                    return View("Versions", viewModel);
                }

                if (apkFile.Length > _fileSizeLimit)
                {
                    ViewBag.Error = "File size exceeds 500 MB limit.";

                    return View("Versions", viewModel);
                }


                if (!Directory.Exists(uploads))
                {
                    Directory.CreateDirectory(uploads);
                }

                // Extract version number using Regex
                string pattern = @"__v(\d+)\.apk$"; // Regex pattern to match version number
                var match = Regex.Match(apkFile.FileName, pattern);

                if (!match.Success)
                {
                    ViewBag.Error = "File name invalid.";
                    return View("Versions", viewModel);
                }

                // Extract version number from the uploaded file name
                string versionNumber = match.Groups[1].Value;

                // Check if a file with the same version number exists
                var existingFiles = Directory.GetFiles(uploads)
                    .Where(file => Path.GetFileName(file).Contains($"__v{versionNumber}.apk"));

                if (existingFiles.Any())
                {
                    ViewBag.Error = $"A file with version '{versionNumber}' already exists.";
                    return View("Versions", viewModel);
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
                    ViewBag.Error = $"File upload failed: {ex.Message}";
                }


                // Refresh the view model. 28.06.2024 Janko Jovièiæ
                viewModel = Methods.FindAndReturnModelVersions(uploads, applicationModel);


                return View("Versions", viewModel);
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                ViewBag.Error = ex.Message;
                return View("Dashboard");
            }
        }





        [HttpPost]
        public async Task<IActionResult> DeleteApk(string apkToDelete, int applicationId)
        {
            var applicationModel = await _context.Applications.FindAsync(applicationId);
            var uploadsPath = Path.Combine(_env.ContentRootPath, "ApplicationData", applicationModel.ApplicationName);
            var viewModel = Methods.FindAndReturnModelVersions(uploadsPath, applicationModel);

            try
            {

                var sessionGuid = HttpContext.Request.Cookies["SessionGuid"];

                // Example: Retrieve session data from service
                var sessionData = _sessionService.GetSessionByGuid(sessionGuid ?? string.Empty);

                // Pass session data to ViewData or ViewBag
                if (sessionData != null && sessionData?.ExpiryTime > DateTime.Now)
                {
                    ViewData["SessionData"] = sessionData;
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }

                if (applicationModel == null)
                {
                    ViewBag.Error = "No application model found.";
                    return View("Versions", viewModel);
                }

                if (string.IsNullOrEmpty(apkToDelete))
                {
                    ViewBag.Error = "No APK file selected for deletion.";
                    return View("Versions", viewModel);
                }

                // Construct the directory path where APKs are stored

                // Check if the directory exists
                if (!Directory.Exists(uploadsPath))
                {
                    ViewBag.Error = "Uploads directory not found.";
                    return View("Versions", viewModel);
                }

                var filesToDeleteDebug = Directory.GetFiles(uploadsPath);

                // Search for files matching the pattern in the directory
                var filesToDelete = Directory.GetFiles(uploadsPath)
                    .Where(file => Path.GetFileName(file).Contains($"__v{apkToDelete}"));

                if (filesToDelete.Count() == 0)
                {
                    ViewBag.Error = $"No APK files found starting with '__v{apkToDelete}'.";
                    return View("Versions", viewModel);
                }

                // Delete each found file
                foreach (var file in filesToDelete)
                {
                    System.IO.File.Delete(file);
                }

                ViewBag.Message = $"APK file starting deleted successfully.";
                // Refresh the view model. 28.06.2024 Janko Jovièiæ
                viewModel = Methods.FindAndReturnModelVersions(uploadsPath, applicationModel);

                return View("Versions", viewModel);

            }

            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                ViewBag.Error = $"Error deleting APK file: {ex.Message}";
                return View("Versions", viewModel);

            }
        }

    }
}

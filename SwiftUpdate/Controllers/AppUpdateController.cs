using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using SwiftUpdate.Helpers;
using SwiftUpdate.Models;

[ApiController]
[Route("api/app")]
public class AppUpdateController : ControllerBase
{
    private readonly IWebHostEnvironment _env; // Assuming you need access to environment
    private readonly SwiftUpdateContext _context; // Your database context

    public AppUpdateController(IWebHostEnvironment env, SwiftUpdateContext context)
    {
        _env = env;
        _context = context;
    }


    [HttpGet("download-update")]
    public IActionResult DownloadUpdate(string applicationName)
    {
        try
        {
            if (string.IsNullOrEmpty(applicationName))
            {
                return BadRequest("Application name is required.");
            }

            var uploadsPath = Path.Combine(_env.ContentRootPath, "ApplicationData", applicationName);

            var apkFilePath = Methods.FindAndReturnUpdatePath(uploadsPath);

            if (string.IsNullOrEmpty(apkFilePath))
            {
                return NotFound("No update found for the specified application.");
            }

            // Serve the file for download
            var fileBytes = System.IO.File.ReadAllBytes(apkFilePath);
            var fileName = Path.GetFileName(apkFilePath);
            return File(fileBytes, "application/vnd.android.package-archive", fileName);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }


    [HttpGet("check-for-update")]
    public IActionResult CheckForUpdate(string applicationName, int versionCode)
    {
        try
        {
            if (string.IsNullOrEmpty(applicationName))
            {
                return BadRequest("Application name is required.");
            }

            var uploadsPath = Path.Combine(_env.ContentRootPath, "ApplicationData", applicationName);

            var versions = Methods.FindAndReturnModelVersionsApi(uploadsPath);

            if (versions == null || versions.Count == 0)
            {
                return NotFound("No versions found for the specified application.");
            }

            var currentMax = versions.Max();

            if (currentMax > versionCode)
            {
                return Ok("New update available!");
            }
            else
            {
                return Ok("No new updates available!");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }
}

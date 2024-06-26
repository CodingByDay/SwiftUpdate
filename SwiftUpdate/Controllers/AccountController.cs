using Microsoft.AspNetCore.Mvc;
using SwiftUpdate.Models;
using SwiftUpdate.ViewModels;
using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using SwiftUpdate.Services;

public class AccountController : Controller
{
    private readonly SwiftUpdateContext _context;
    private readonly PasswordHasher<UserModel> _passwordHasher;
    private readonly SessionService _sessionService;

    public AccountController(SwiftUpdateContext context, PasswordHasher<UserModel> passwordHasher, SessionService sessionService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _sessionService = sessionService;
    }

    // GET: /Account/Login
    [HttpGet]
    public IActionResult Login()
    {
        ViewData["Account"] = true;
        // Get session information
        var sessionGuid = HttpContext.Request.Cookies["SessionGuid"]; // Replace with your session cookie name

        // Example: Retrieve session data from service
        var sessionData = _sessionService.GetSessionByGuid(sessionGuid ?? string.Empty); // Implement this method in your service

        // Pass session data to ViewData or ViewBag
        if (sessionData != null)
        {
            ViewData["SessionData"] = sessionData;
        }
        return View();
    }




    [HttpGet]
    public IActionResult Logout()
    {

        // Get session information
        var sessionGuid = HttpContext.Request.Cookies["SessionGuid"]; // Replace with your session cookie name

        // Example: Retrieve session data from service
        var sessionData = _sessionService.GetSessionByGuid(sessionGuid ?? string.Empty); // Implement this method in your service

        // Pass session data to ViewData or ViewBag
        if (sessionData != null)
        {
            ViewData["SessionData"] = string.Empty;
        }

        _sessionService.DeleteSessionByGuid(sessionGuid ?? string.Empty);

        return RedirectToAction("Login", "Account");

    }




    // POST: /Account/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Validate the user credentials and hash comparison
            var user = _context.Users.FirstOrDefault(u => u.Username == model.Username);

            if (user != null)
            {
                var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);

                if (passwordVerificationResult == PasswordVerificationResult.Success)
                {
                    // Create claims for the user (optional)
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Username)
                        // Add more claims as needed
                    };

                    // Create identity object
                    var userIdentity = new ClaimsIdentity(claims, "login");




                    // Generate a unique session ID (you can use GUID or any unique identifier)
                    string sessionGuid = Guid.NewGuid().ToString();

                    // Set session expiry time (e.g., 30 minutes from now)
                    DateTime expiryTime = DateTime.Now.AddMinutes(30);

                    // Create session using SessionService
                    string ResultGuid = _sessionService.CreateSession(sessionGuid, user.UserId, expiryTime);

                    var cookieOptions = new CookieOptions
                    {
                        // Set other cookie options as needed, like expiration, domain, etc.
                        HttpOnly = true, // This restricts cookie access to HTTP requests only
                        IsEssential = true // This marks the cookie as essential for authentication
                    };

                    // Set the cookie in the response
                    HttpContext.Response.Cookies.Append("SessionGuid", ResultGuid, cookieOptions);

                    return RedirectToAction("Dashboard", "Home"); 
                }
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        }

        // If we got this far, something failed, redisplay form
        return View(model);
    }

    // GET: /Account/Register
    [HttpGet]
    public IActionResult Register()
    {
        ViewData["Account"] = true;

        return View();
    }

    // POST: /Account/Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Check if the username is already taken (for demo purposes, you should add more validation)
            if (_context.Users.Any(u => u.Username == model.Username))
            {
                ModelState.AddModelError("Username", "Username is already taken.");
                return View(model);
            }

            // Hash the password using PasswordHasher
            var hashedPassword = _passwordHasher.HashPassword(null, model.Password);

            // Create a new user
            var newUser = new UserModel
            {
                Username = model.Username,
                PasswordHash = hashedPassword,
                Email = model.Email,
                IsAdmin = true
            };

            // Save the new user to the database
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login"); // Redirect to login page after successful registration
        }

        // If we got this far, something failed, redisplay form
        return View(model);
    }


  
}

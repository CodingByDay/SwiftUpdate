using Microsoft.AspNetCore.Mvc;
using SwiftUpdate.Models;
using SwiftUpdate.ViewModels;
using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

public class AccountController : Controller
{
    private readonly SwiftUpdateContext _context;
    private readonly PasswordHasher<UserModel> _passwordHasher;

    public AccountController(SwiftUpdateContext context, PasswordHasher<UserModel> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    // GET: /Account/Login
    [HttpGet]
    public IActionResult Login()
    {
        return View();
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

    // GET: /Account/Logout
    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction("Index", "Home"); // Redirect to home page after logout
    }
}

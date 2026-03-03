using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MRMstudios.Pages;

public class AdminLoginModel : PageModel
{
    private readonly IConfiguration _configuration;

    public AdminLoginModel(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [BindProperty]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    public void OnGet()
    {
        // Clear any existing session to prevent cached login
        HttpContext.Session.Clear();
    }

    public IActionResult OnPost()
    {
        if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
        {
            TempData["Error"] = "Please enter both username and password.";
            return Page();
        }

        var adminUsername = Environment.GetEnvironmentVariable("ADMIN_USERNAME")
                            ?? _configuration["Admin:Username"]
                            ?? "admin";
        var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD")
                            ?? _configuration["Admin:Password"];

        if (string.IsNullOrWhiteSpace(adminPassword))
        {
            TempData["Error"] = "Admin password is not configured. Set ADMIN_PASSWORD and restart the app.";
            return Page();
        }

        var validCredentials = Username == adminUsername && Password == adminPassword;
        if (validCredentials)
        {
            // Set session cookie for authentication
            HttpContext.Session.SetString("AdminAuthenticated", "true");
            HttpContext.Session.SetString("AdminLoginTime", DateTime.Now.ToString());
            
            return RedirectToPage("/AdminBookings");
        }

        TempData["Error"] = "Invalid username or password. Please try again.";
        Password = string.Empty; // Clear password field for security
        return Page();
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MRMstudios.Pages;

public class AdminLoginModel : PageModel
{
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
        // Hard-coded admin credentials (for demo - use environment variables in production)
        const string adminUsername = "admin";
        const string adminPassword = "MRMstudios2026!Secure";

        if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
        {
            TempData["Error"] = "Please enter both username and password.";
            return Page();
        }

        // Verify credentials
        if (Username == adminUsername && Password == adminPassword)
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

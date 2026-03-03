using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MRMstudios.Services;

namespace MRMstudios.Pages;

public class AdminLoginModel : PageModel
{
    private readonly IAdminAuthService _adminAuthService;

    public AdminLoginModel(IAdminAuthService adminAuthService)
    {
        _adminAuthService = adminAuthService;
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

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
        {
            TempData["Error"] = "Please enter both username and password.";
            return Page();
        }

        // Verify credentials against hashed stored password
        var validCredentials = await _adminAuthService.ValidateCredentialsAsync(Username, Password);
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

    public async Task<IActionResult> OnPostGeneratePasswordAsync()
    {
        HttpContext.Session.Clear();

        var sent = await _adminAuthService.GenerateAndEmailNewPasswordAsync();
        if (sent)
        {
            TempData["Success"] = "A new admin password was sent to mel.dimplz@gmail.com.";
        }
        else
        {
            TempData["Error"] = "Could not send reset email. Check SMTP settings and try again.";
        }

        return Page();
    }
}

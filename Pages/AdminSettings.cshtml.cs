using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MRMstudios.Models;
using MRMstudios.Services;

namespace MRMstudios.Pages
{
    public class AdminSettingsModel : PageModel
    {
        private readonly IContentService _contentService;
        private readonly IEmailSettingsService _emailSettingsService;
        private readonly IEmailService _emailService;
        private readonly ILogger<AdminSettingsModel> _logger;

        public AdminSettingsModel(IContentService contentService, IEmailSettingsService emailSettingsService, IEmailService emailService, ILogger<AdminSettingsModel> logger)
        {
            _contentService = contentService;
            _emailSettingsService = emailSettingsService;
            _emailService = emailService;
            _logger = logger;
        }

        public List<Service> Services { get; set; } = new();
        [BindProperty]
        public EmailSettings EmailSettings { get; set; } = new();
        public string? Message { get; set; }
        public string? Error { get; set; }

        [BindProperty]
        public Service NewService { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            if (HttpContext.Session.GetString("AdminAuthenticated") != "true")
            {
                return RedirectToPage("/AdminLogin");
            }

            Services = await _contentService.GetServicesAsync();
            EmailSettings = await _emailSettingsService.GetSettingsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAddServiceAsync()
        {
            if (HttpContext.Session.GetString("AdminAuthenticated") != "true")
            {
                return RedirectToPage("/AdminLogin");
            }

            if (string.IsNullOrWhiteSpace(NewService.Name) || NewService.Price <= 0)
            {
                Error = "Please enter a valid service name and price.";
                Services = await _contentService.GetServicesAsync();
                return Page();
            }

            try
            {
                var services = await _contentService.GetServicesAsync();
                NewService.Description = NewService.Description?.Trim() ?? string.Empty;
                NewService.Name = NewService.Name.Trim();
                NewService.Id = services.Any() ? services.Max(s => s.Id) + 1 : 1;
                services.Add(NewService);
                await _contentService.SaveServicesAsync(services);
                Message = $"Service '{NewService.Name}' added successfully!";
                NewService = new();
            }
            catch (Exception ex)
            {
                Error = $"Error adding service: {ex.Message}";
                _logger.LogError(ex, "Error adding service");
            }

            Services = await _contentService.GetServicesAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateServiceAsync(int id, string name, decimal price, string description)
        {
            if (HttpContext.Session.GetString("AdminAuthenticated") != "true")
            {
                return RedirectToPage("/AdminLogin");
            }

            if (string.IsNullOrWhiteSpace(name) || price <= 0)
            {
                Error = "Please enter a valid service name and price.";
                Services = await _contentService.GetServicesAsync();
                return Page();
            }

            try
            {
                var services = await _contentService.GetServicesAsync();
                var service = services.FirstOrDefault(s => s.Id == id);
                if (service == null)
                {
                    Error = "Service not found.";
                    Services = services;
                    return Page();
                }

                service.Name = name.Trim();
                service.Price = price;
                service.Description = description?.Trim() ?? string.Empty;
                await _contentService.SaveServicesAsync(services);
                Message = $"Service '{service.Name}' updated successfully!";
            }
            catch (Exception ex)
            {
                Error = $"Error updating service: {ex.Message}";
                _logger.LogError(ex, "Error updating service");
            }

            Services = await _contentService.GetServicesAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteServiceAsync(int id)
        {
            if (HttpContext.Session.GetString("AdminAuthenticated") != "true")
            {
                return RedirectToPage("/AdminLogin");
            }

            try
            {
                var services = await _contentService.GetServicesAsync();
                var service = services.FirstOrDefault(s => s.Id == id);
                if (service == null)
                {
                    Error = "Service not found.";
                }
                else
                {
                    services.RemoveAll(s => s.Id == id);
                    await _contentService.SaveServicesAsync(services);
                    Message = $"Service '{service.Name}' deleted successfully!";
                }
            }
            catch (Exception ex)
            {
                Error = $"Error deleting service: {ex.Message}";
                _logger.LogError(ex, "Error deleting service");
            }

            Services = await _contentService.GetServicesAsync();
            EmailSettings = await _emailSettingsService.GetSettingsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostSaveEmailSettingsAsync()
        {
            if (HttpContext.Session.GetString("AdminAuthenticated") != "true")
            {
                return RedirectToPage("/AdminLogin");
            }

            if (string.IsNullOrWhiteSpace(EmailSettings.SmtpHost) || EmailSettings.SmtpPort <= 0)
            {
                Error = "Please enter valid SMTP host and port values.";
                Services = await _contentService.GetServicesAsync();
                EmailSettings = await _emailSettingsService.GetSettingsAsync();
                return Page();
            }

            try
            {
                await _emailSettingsService.SaveSettingsAsync(EmailSettings);
                Message = "Email settings saved successfully.";
            }
            catch (Exception ex)
            {
                Error = $"Error saving email settings: {ex.Message}";
                _logger.LogError(ex, "Error saving email settings");
            }

            Services = await _contentService.GetServicesAsync();
            EmailSettings = await _emailSettingsService.GetSettingsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostSendTestEmailAsync()
        {
            if (HttpContext.Session.GetString("AdminAuthenticated") != "true")
            {
                return RedirectToPage("/AdminLogin");
            }

            try
            {
                await _emailSettingsService.SaveSettingsAsync(EmailSettings);
                var settings = await _emailSettingsService.GetSettingsAsync();
                var sent = await _emailService.SendBookingNotificationToOwnerAsync(
                    "Test User",
                    "test@example.com",
                    "+0000000000",
                    "Test Booking",
                    DateTime.Now.AddDays(7),
                    "This is a test message.");

                if (sent)
                {
                    Message = "Test email sent successfully to the configured admin address.";
                }
                else
                {
                    Error = "Test email failed. Check SMTP settings and server connectivity.";
                }
            }
            catch (Exception ex)
            {
                Error = $"Error sending test email: {ex.Message}";
                _logger.LogError(ex, "Error sending test email");
            }

            Services = await _contentService.GetServicesAsync();
            EmailSettings = await _emailSettingsService.GetSettingsAsync();
            return Page();
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MRMstudios.Models;
using MRMstudios.Services;
using System.Text.Json;

namespace MRMstudios.Pages
{
    public class AdminSettingsModel : PageModel
    {
        private readonly IBookingService _bookingService;
        private readonly ILogger<AdminSettingsModel> _logger;

        public AdminSettingsModel(IBookingService bookingService, ILogger<AdminSettingsModel> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }

        public List<Service> Services { get; set; } = new();
        public string? Message { get; set; }
        public string? Error { get; set; }

        [BindProperty]
        public Service NewService { get; set; } = new();

        [BindProperty]
        public int? EditServiceId { get; set; }

        public IActionResult OnGet()
        {
            // Check authentication
            var isAuthenticated = HttpContext.Session.GetString("AdminAuthenticated") == "true";
            if (!isAuthenticated)
            {
                return RedirectToPage("/AdminLogin");
            }

            LoadServices();
            return Page();
        }

        public IActionResult OnPostAddService()
        {
            var isAuthenticated = HttpContext.Session.GetString("AdminAuthenticated") == "true";
            if (!isAuthenticated)
            {
                return RedirectToPage("/AdminLogin");
            }

            if (string.IsNullOrWhiteSpace(NewService.Name) || NewService.Price <= 0)
            {
                Error = "Please enter a valid service name and price.";
                LoadServices();
                return Page();
            }

            try
            {
                NewService.Description = NewService.Description?.Trim() ?? string.Empty;
                NewService.Name = NewService.Name.Trim();
                
                var services = LoadServicesFromJson();
                NewService.Id = services.Count > 0 ? services.Max(s => s.Id) + 1 : 1;
                services.Add(NewService);
                
                SaveServicesToJson(services);
                Message = $"Service '{NewService.Name}' added successfully!";
                NewService = new();
                _logger.LogInformation("Service added by admin: {ServiceName}", NewService.Name);
            }
            catch (Exception ex)
            {
                Error = $"Error adding service: {ex.Message}";
                _logger.LogError(ex, "Error adding service");
            }

            LoadServices();
            return Page();
        }

        public IActionResult OnPostUpdateService(int id, string name, decimal price, string description)
        {
            var isAuthenticated = HttpContext.Session.GetString("AdminAuthenticated") == "true";
            if (!isAuthenticated)
            {
                return RedirectToPage("/AdminLogin");
            }

            if (string.IsNullOrWhiteSpace(name) || price <= 0)
            {
                Error = "Please enter a valid service name and price.";
                LoadServices();
                return Page();
            }

            try
            {
                var services = LoadServicesFromJson();
                var service = services.FirstOrDefault(s => s.Id == id);
                
                if (service == null)
                {
                    Error = "Service not found.";
                    LoadServices();
                    return Page();
                }

                service.Name = name.Trim();
                service.Price = price;
                service.Description = description?.Trim() ?? string.Empty;

                SaveServicesToJson(services);
                Message = $"Service '{service.Name}' updated successfully!";
                _logger.LogInformation("Service updated by admin: {ServiceName}", service.Name);
            }
            catch (Exception ex)
            {
                Error = $"Error updating service: {ex.Message}";
                _logger.LogError(ex, "Error updating service");
            }

            LoadServices();
            return Page();
        }

        public IActionResult OnPostDeleteService(int id)
        {
            var isAuthenticated = HttpContext.Session.GetString("AdminAuthenticated") == "true";
            if (!isAuthenticated)
            {
                return RedirectToPage("/AdminLogin");
            }

            try
            {
                var services = LoadServicesFromJson();
                var service = services.FirstOrDefault(s => s.Id == id);
                
                if (service == null)
                {
                    Error = "Service not found.";
                }
                else
                {
                    services.RemoveAll(s => s.Id == id);
                    SaveServicesToJson(services);
                    Message = $"Service '{service.Name}' deleted successfully!";
                    _logger.LogInformation("Service deleted by admin: {ServiceName}", service.Name);
                }
            }
            catch (Exception ex)
            {
                Error = $"Error deleting service: {ex.Message}";
                _logger.LogError(ex, "Error deleting service");
            }

            LoadServices();
            return Page();
        }

        private void LoadServices()
        {
            try
            {
                Services = _bookingService.GetServices();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading services");
                Services = new List<Service>();
            }
        }

        private List<Service> LoadServicesFromJson()
        {
            return _bookingService.GetServices();
        }

        private void SaveServicesToJson(List<Service> services)
        {
            // Services are stored in memory in BookingService
            // For a production app, you'd want to persist these to a JSON file
            // This is a simplified version - in production, create a separate settings service
        }
    }
}

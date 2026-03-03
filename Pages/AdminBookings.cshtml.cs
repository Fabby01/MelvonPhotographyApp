using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MRMstudios.Models;
using MRMstudios.Services;

namespace MRMstudios.Pages
{
    public class AdminBookingsModel : PageModel
    {
        private readonly IBookingService _bookingService;
        private readonly IEmailService _emailService;
        private readonly ILogger<AdminBookingsModel> _logger;

        public AdminBookingsModel(IBookingService bookingService, IEmailService emailService, ILogger<AdminBookingsModel> logger)
        {
            _bookingService = bookingService;
            _emailService = emailService;
            _logger = logger;
        }

        public List<Booking> Bookings { get; set; } = new();
        public string? Message { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Check authentication
            var isAuthenticated = HttpContext.Session.GetString("AdminAuthenticated") == "true";
            if (!isAuthenticated)
            {
                return RedirectToPage("/AdminLogin");
            }

            Bookings = await _bookingService.GetAllBookingsAsync();
            Bookings = Bookings.OrderByDescending(b => b.BookingDate).ToList();
            return Page();
        }

        public IActionResult OnPostLogout()
        {
            // Clear admin session
            HttpContext.Session.Clear();
            return RedirectToPage("/AdminLogin");
        }

        public async Task<IActionResult> OnPostConfirmAsync(int id)
        {
            // Verify authentication
            var isAuthenticated = HttpContext.Session.GetString("AdminAuthenticated") == "true";
            if (!isAuthenticated)
            {
                return RedirectToPage("/AdminLogin");
            }

            var booking = await _bookingService.UpdateBookingStatusAsync(id, "Confirmed");
            if (booking != null)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _emailService.SendConfirmationEmailToClientAsync(
                            booking.Email,
                            booking.FullName,
                            booking.ServiceType,
                            booking.PreferredDate);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send async confirmation email for booking id {BookingId}", booking.Id);
                    }
                });
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            // Verify authentication
            var isAuthenticated = HttpContext.Session.GetString("AdminAuthenticated") == "true";
            if (!isAuthenticated)
            {
                return RedirectToPage("/AdminLogin");
            }

            await _bookingService.UpdateBookingStatusAsync(id, "Cancelled");
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            // Verify authentication
            var isAuthenticated = HttpContext.Session.GetString("AdminAuthenticated") == "true";
            if (!isAuthenticated)
            {
                return RedirectToPage("/AdminLogin");
            }

            await _bookingService.DeleteBookingAsync(id);
            return RedirectToPage();
        }
    }
}

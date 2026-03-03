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

        public AdminBookingsModel(IBookingService bookingService, IEmailService emailService)
        {
            _bookingService = bookingService;
            _emailService = emailService;
        }

        public List<Booking> Bookings { get; set; } = new();
        public string? Message { get; set; }

        public async Task OnGetAsync()
        {
            // Check authentication
            var isAuthenticated = HttpContext.Session.GetString("AdminAuthenticated") == "true";
            if (!isAuthenticated)
            {
                return;
            }

            Bookings = await _bookingService.GetAllBookingsAsync();
            Bookings = Bookings.OrderByDescending(b => b.BookingDate).ToList();
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

            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking != null)
            {
                booking.Status = "Confirmed";
                await _bookingService.UpdateBookingAsync(booking);

                // Send confirmation email to client
                await _emailService.SendConfirmationEmailToClientAsync(
                    booking.Email, 
                    booking.FullName, 
                    booking.ServiceType, 
                    booking.PreferredDate);
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

            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking != null)
            {
                booking.Status = "Cancelled";
                await _bookingService.UpdateBookingAsync(booking);
            }
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

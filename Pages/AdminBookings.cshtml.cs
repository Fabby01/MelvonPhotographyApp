using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MelvonPhotographyApp.Models;
using MelvonPhotographyApp.Services;

namespace MelvonPhotographyApp.Pages
{
    public class AdminBookingsModel : PageModel
    {
        private readonly IBookingService _bookingService;

        public AdminBookingsModel(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        public List<Booking> Bookings { get; set; } = new();
        public string? Message { get; set; }

        public async Task OnGetAsync()
        {
            Bookings = await _bookingService.GetAllBookingsAsync();
            Bookings = Bookings.OrderByDescending(b => b.BookingDate).ToList();
        }

        public async Task<IActionResult> OnPostConfirmAsync(int id)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking != null)
            {
                booking.Status = "Confirmed";
                await _bookingService.UpdateBookingAsync(booking);
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
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
            await _bookingService.DeleteBookingAsync(id);
            return RedirectToPage();
        }
    }
}

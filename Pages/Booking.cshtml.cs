using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MelvonPhotographyApp.Models;
using MelvonPhotographyApp.Services;

namespace MelvonPhotographyApp.Pages
{
    public class BookingModel : PageModel
    {
        private readonly IBookingService _bookingService;

        public BookingModel(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [BindProperty]
        public Booking Booking { get; set; } = new();

        public List<Service> Services { get; set; } = new();
        public string? Message { get; set; }
        public string? Error { get; set; }

        public void OnGet()
        {
            Services = _bookingService.GetServices();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Services = _bookingService.GetServices();

            if (!ModelState.IsValid)
            {
                Error = "Please fill in all required fields correctly.";
                return Page();
            }

            if (Booking.PreferredDate < DateTime.Today)
            {
                Error = "Please select a future date.";
                return Page();
            }

            try
            {
                var bookingId = await _bookingService.CreateBookingAsync(Booking);
                Message = $"✅ Booking submitted successfully! Confirmation ID: {bookingId}. We'll contact you shortly to confirm.";
                Booking = new();
                return Page();
            }
            catch
            {
                Error = "An error occurred while processing your booking. Please try again.";
                return Page();
            }
        }
    }
}

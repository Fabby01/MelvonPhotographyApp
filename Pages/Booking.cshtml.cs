using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MRMstudios.Models;
using MRMstudios.Services;

namespace MRMstudios.Pages
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
        public List<DateTime> AvailableDates { get; set; } = new();

        public async Task OnGetAsync()
        {
            Services = _bookingService.GetServices();
            AvailableDates = await _bookingService.GetAvailableDatesAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Services = _bookingService.GetServices();
            AvailableDates = await _bookingService.GetAvailableDatesAsync();

            if (!ModelState.IsValid)
            {
                Error = "Please fill in all required fields correctly.";
                return Page();
            }

            if (Booking.PreferredDate < DateTime.Today.AddDays(7))
            {
                Error = "Please select a date at least 7 days in the future.";
                return Page();
            }

            var isAvailable = await _bookingService.IsDateAvailableAsync(Booking.PreferredDate);
            if (!isAvailable)
            {
                Error = "That date is already booked. Please select another date from the available dates.";
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

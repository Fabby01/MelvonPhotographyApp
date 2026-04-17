using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MRMstudios.Models;
using MRMstudios.Services;

namespace MRMstudios.Pages
{
    public class BookingConfirmationModel : PageModel
    {
        private readonly IBookingService _bookingService;
        private readonly ILogger<BookingConfirmationModel> _logger;

        public BookingConfirmationModel(IBookingService bookingService, ILogger<BookingConfirmationModel> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public Booking Booking { get; set; } = new();
        public decimal ServicePrice { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (Id <= 0)
            {
                return RedirectToPage("/Index");
            }

            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(Id);
                if (booking == null)
                {
                    return RedirectToPage("/Index");
                }

                Booking = booking;

                // Get service price
                var services = _bookingService.GetServices();
                var service = services.FirstOrDefault(s => s.Name == booking.ServiceType);
                ServicePrice = service?.Price ?? 0;

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading booking confirmation for ID {BookingId}", Id);
                return RedirectToPage("/Index");
            }
        }
    }
}

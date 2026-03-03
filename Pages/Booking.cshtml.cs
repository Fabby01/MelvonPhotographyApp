using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MRMstudios.Models;
using MRMstudios.Services;

namespace MRMstudios.Pages
{
    public class BookingModel : PageModel
    {
        private readonly IBookingService _bookingService;
        private readonly IEmailService _emailService;

        public BookingModel(IBookingService bookingService, IEmailService emailService)
        {
            _bookingService = bookingService;
            _emailService = emailService;
        }

        [BindProperty]
        public Booking Booking { get; set; } = new();

        public List<Service> Services { get; set; } = new();
        public string? Message { get; set; }
        public string? Error { get; set; }
        public List<DateTime> AvailableDates { get; set; } = new();
        public int TotalBookings { get; set; }

        public async Task OnGetAsync()
        {
            Services = _bookingService.GetServices();
            AvailableDates = await _bookingService.GetAvailableDatesAsync();
            TotalBookings = await _bookingService.GetBookingCountAsync();
            
            // Run cleanup task in background (remove old bookings)
            _ = _bookingService.CleanupOldBookingsAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Services = _bookingService.GetServices();
            AvailableDates = await _bookingService.GetAvailableDatesAsync();
            TotalBookings = await _bookingService.GetBookingCountAsync();

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
                Error = "That date is not available. Please select another date from the available dates.";
                return Page();
            }

            try
            {
                // Create the booking
                var bookingId = await _bookingService.CreateBookingAsync(Booking);
                
                // Get the service details for the email
                var service = Services.FirstOrDefault(s => s.Name == Booking.ServiceType);
                var price = service?.Price ?? 0;

                // Send confirmation email to client
                var clientEmailSent = await _emailService.SendBookingConfirmationAsync(
                    Booking.Email,
                    Booking.FullName,
                    bookingId,
                    Booking.ServiceType,
                    Booking.PreferredDate,
                    price
                );

                // Send notification email to business owner
                var ownerEmailSent = await _emailService.SendBookingNotificationToOwnerAsync(
                    Booking.FullName,
                    Booking.Email,
                    Booking.PhoneNumber,
                    Booking.ServiceType,
                    Booking.PreferredDate,
                    Booking.SpecialNotes
                );

                var emailStatus = (clientEmailSent && ownerEmailSent) 
                    ? "✅ Confirmation emails have been sent!" 
                    : "⚠️ Confirmation received, but emails could not be sent at this time.";

                Message = $"✅ Booking submitted successfully! Confirmation ID: {bookingId}. {emailStatus} We'll contact you shortly to confirm.";
                Booking = new();
                TotalBookings = await _bookingService.GetBookingCountAsync();
                return Page();
            }
            catch (Exception ex)
            {
                Error = $"An error occurred while processing your booking: {ex.Message}";
                return Page();
            }
        }
    }
}

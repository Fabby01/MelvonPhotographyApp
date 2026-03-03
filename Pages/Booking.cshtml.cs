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
        private readonly ILogger<BookingModel> _logger;

        public BookingModel(
            IBookingService bookingService,
            IEmailService emailService,
            ILogger<BookingModel> logger)
        {
            _bookingService = bookingService;
            _emailService = emailService;
            _logger = logger;
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

            Booking.FullName = Booking.FullName?.Trim() ?? string.Empty;
            Booking.Email = Booking.Email?.Trim() ?? string.Empty;
            Booking.PhoneNumber = Booking.PhoneNumber?.Trim() ?? string.Empty;
            Booking.ServiceType = Booking.ServiceType?.Trim() ?? string.Empty;
            Booking.SpecialNotes = Booking.SpecialNotes?.Trim() ?? string.Empty;

            if (!ModelState.IsValid)
            {
                Error = "Please fill in all required fields correctly.";
                return Page();
            }

            if (!Services.Any(s => s.Name == Booking.ServiceType))
            {
                Error = "Please select a valid service.";
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
                var bookingId = await _bookingService.CreateBookingAsync(Booking);
                var service = Services.FirstOrDefault(s => s.Name == Booking.ServiceType);
                var price = service?.Price ?? 0;

                var bookingSnapshot = new Booking
                {
                    Id = bookingId,
                    FullName = Booking.FullName,
                    Email = Booking.Email,
                    PhoneNumber = Booking.PhoneNumber,
                    ServiceType = Booking.ServiceType,
                    PreferredDate = Booking.PreferredDate,
                    SpecialNotes = Booking.SpecialNotes
                };

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await Task.WhenAll(
                            _emailService.SendBookingConfirmationAsync(
                                bookingSnapshot.Email,
                                bookingSnapshot.FullName,
                                bookingSnapshot.Id,
                                bookingSnapshot.ServiceType,
                                bookingSnapshot.PreferredDate,
                                price),
                            _emailService.SendBookingNotificationToOwnerAsync(
                                bookingSnapshot.FullName,
                                bookingSnapshot.Email,
                                bookingSnapshot.PhoneNumber,
                                bookingSnapshot.ServiceType,
                                bookingSnapshot.PreferredDate,
                                bookingSnapshot.SpecialNotes));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Background booking email processing failed for booking id {BookingId}", bookingSnapshot.Id);
                    }
                });

                Message = $"Booking submitted successfully. Confirmation ID: {bookingId}. We'll contact you shortly.";
                Booking = new();
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

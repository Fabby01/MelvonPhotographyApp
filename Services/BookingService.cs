using MelvonPhotographyApp.Models;
using System.Text.Json;

namespace MelvonPhotographyApp.Services
{
    public interface IBookingService
    {
        Task<List<Booking>> GetAllBookingsAsync();
        Task<Booking?> GetBookingByIdAsync(int id);
        Task<int> CreateBookingAsync(Booking booking);
        Task<bool> UpdateBookingAsync(Booking booking);
        Task<bool> DeleteBookingAsync(int id);
        List<Service> GetServices();
        Task<List<DateTime>> GetAvailableDatesAsync(int monthsAhead = 3);
        Task<bool> IsDateAvailableAsync(DateTime date);
    }

    public class BookingService : IBookingService
    {
        private readonly string _bookingsFilePath;
        private readonly List<Service> _services;

        public BookingService(IWebHostEnvironment environment)
        {
            _bookingsFilePath = Path.Combine(environment.WebRootPath, "bookings.json");
            _services = new List<Service>
            {
                new Service { Id = 1, Name = "Wedding Photography", Price = 800, Description = "Full day wedding coverage with multiple locations" },
                new Service { Id = 2, Name = "Portrait Session", Price = 300, Description = "Professional portrait session with retouching" },
                new Service { Id = 3, Name = "Corporate Photography", Price = 500, Description = "Corporate event or headshot photography" },
                new Service { Id = 4, Name = "Event Photography", Price = 600, Description = "Birthday, anniversary, or special event coverage" }
            };
        }

        public async Task<List<Booking>> GetAllBookingsAsync()
        {
            try
            {
                if (!File.Exists(_bookingsFilePath))
                    return new List<Booking>();

                var json = await File.ReadAllTextAsync(_bookingsFilePath);
                return JsonSerializer.Deserialize<List<Booking>>(json) ?? new List<Booking>();
            }
            catch
            {
                return new List<Booking>();
            }
        }

        public async Task<Booking?> GetBookingByIdAsync(int id)
        {
            var bookings = await GetAllBookingsAsync();
            return bookings.FirstOrDefault(b => b.Id == id);
        }

        public async Task<int> CreateBookingAsync(Booking booking)
        {
            var bookings = await GetAllBookingsAsync();
            booking.Id = bookings.Any() ? bookings.Max(b => b.Id) + 1 : 1;
            booking.BookingDate = DateTime.Now;
            bookings.Add(booking);

            await SaveBookingsAsync(bookings);
            return booking.Id;
        }

        public async Task<bool> UpdateBookingAsync(Booking booking)
        {
            var bookings = await GetAllBookingsAsync();
            var existingBooking = bookings.FirstOrDefault(b => b.Id == booking.Id);

            if (existingBooking == null)
                return false;

            existingBooking.Status = booking.Status;
            existingBooking.FullName = booking.FullName;
            existingBooking.Email = booking.Email;
            existingBooking.PhoneNumber = booking.PhoneNumber;
            existingBooking.ServiceType = booking.ServiceType;
            existingBooking.PreferredDate = booking.PreferredDate;
            existingBooking.SpecialNotes = booking.SpecialNotes;

            await SaveBookingsAsync(bookings);
            return true;
        }

        public async Task<bool> DeleteBookingAsync(int id)
        {
            var bookings = await GetAllBookingsAsync();
            var booking = bookings.FirstOrDefault(b => b.Id == id);

            if (booking == null)
                return false;

            bookings.Remove(booking);
            await SaveBookingsAsync(bookings);
            return true;
        }

        public List<Service> GetServices()
        {
            return _services;
        }

        public async Task<List<DateTime>> GetAvailableDatesAsync(int monthsAhead = 3)
        {
            var bookings = await GetAllBookingsAsync();
            var bookedDates = bookings
                .Where(b => b.Status != "Cancelled")
                .Select(b => b.PreferredDate.Date)
                .Distinct()
                .ToList();

            var availableDates = new List<DateTime>();
            var today = DateTime.Today.AddDays(7); // Start 7 days from now
            var endDate = today.AddMonths(monthsAhead);

            for (var date = today; date <= endDate; date = date.AddDays(1))
            {
                // Skip weekends if desired (commented out - feel free to enable)
                // if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                //     continue;

                if (!bookedDates.Contains(date))
                {
                    availableDates.Add(date);
                }
            }

            return availableDates;
        }

        public async Task<bool> IsDateAvailableAsync(DateTime date)
        {
            var bookings = await GetAllBookingsAsync();
            var isBooked = bookings
                .Where(b => b.Status != "Cancelled")
                .Any(b => b.PreferredDate.Date == date.Date);

            return !isBooked && date.Date >= DateTime.Today.AddDays(7);
        }

        private async Task SaveBookingsAsync(List<Booking> bookings)
        {
            var json = JsonSerializer.Serialize(bookings, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_bookingsFilePath, json);
        }
    }
}

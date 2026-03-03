using MRMstudios.Models;
using System.Text.Json;

namespace MRMstudios.Services
{
    public interface IBookingService
    {
        Task<List<Booking>> GetAllBookingsAsync();
        Task<Booking?> GetBookingByIdAsync(int id);
        Task<int> CreateBookingAsync(Booking booking);
        Task<bool> UpdateBookingAsync(Booking booking);
        Task<Booking?> UpdateBookingStatusAsync(int id, string status);
        Task<bool> DeleteBookingAsync(int id);
        List<Service> GetServices();
        Task<List<DateTime>> GetAvailableDatesAsync(int monthsAhead = 3);
        Task<bool> IsDateAvailableAsync(DateTime date);
        Task<bool> CleanupOldBookingsAsync();
        Task<int> GetBookingCountAsync();
    }

    public class BookingService : IBookingService
    {
        private readonly string _bookingsFilePath;
        private readonly List<Service> _services;
        private readonly ILogger<BookingService> _logger;
        private readonly SemaphoreSlim _fileLock = new(1, 1);

        public BookingService(IWebHostEnvironment environment, IConfiguration configuration, ILogger<BookingService> logger)
        {
            _logger = logger;

            var configuredPath = Environment.GetEnvironmentVariable("BOOKINGS_FILE_PATH") ?? configuration["Storage:BookingsFilePath"];
            _bookingsFilePath = ResolveWritableBookingsPath(environment, configuredPath);
            TryMigrateLegacyBookingsFile(environment);
            _logger.LogInformation("Using bookings file path: {BookingsFilePath}", _bookingsFilePath);

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
            await _fileLock.WaitAsync();
            try
            {
                if (!File.Exists(_bookingsFilePath))
                    return new List<Booking>();

                var json = await File.ReadAllTextAsync(_bookingsFilePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return new List<Booking>();
                }

                return JsonSerializer.Deserialize<List<Booking>>(json) ?? new List<Booking>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read bookings from {BookingsFilePath}", _bookingsFilePath);
                return new List<Booking>();
            }
            finally
            {
                _fileLock.Release();
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

        public async Task<Booking?> UpdateBookingStatusAsync(int id, string status)
        {
            var bookings = await GetAllBookingsAsync();
            var booking = bookings.FirstOrDefault(b => b.Id == id);
            if (booking == null)
            {
                return null;
            }

            booking.Status = status;
            await SaveBookingsAsync(bookings);
            return booking;
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
                .ToList();

            var availableDates = new List<DateTime>();
            var today = DateTime.Today.AddDays(7); // Start 7 days from now
            var endDate = today.AddMonths(monthsAhead);

            for (var date = today; date <= endDate; date = date.AddDays(1))
            {
                // Allow multiple bookings per day - removed the booking limit
                // Just ensure we track booked dates for reference
                availableDates.Add(date);
            }

            return availableDates;
        }

        public async Task<bool> IsDateAvailableAsync(DateTime date)
        {
            await Task.CompletedTask; // Satisfy async requirement
            // Allow multiple bookings on the same date
            return date.Date >= DateTime.Today.AddDays(7);
        }

        public async Task<bool> CleanupOldBookingsAsync()
        {
            try
            {
                var bookings = await GetAllBookingsAsync();
                var originalCount = bookings.Count;

                // Remove cancelled bookings older than 30 days
                var thirtyDaysAgo = DateTime.Now.AddDays(-30);
                bookings = bookings
                    .Where(b => !(b.Status == "Cancelled" && b.BookingDate < thirtyDaysAgo))
                    .ToList();

                // Remove completed bookings (past date) older than 90 days after completion
                var ninetyDaysAgo = DateTime.Now.AddDays(-90);
                bookings = bookings
                    .Where(b => !(b.PreferredDate < DateTime.Today && b.BookingDate < ninetyDaysAgo))
                    .ToList();

                if (bookings.Count < originalCount)
                {
                    await SaveBookingsAsync(bookings);
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<int> GetBookingCountAsync()
        {
            var bookings = await GetAllBookingsAsync();
            return bookings.Count;
        }

        private async Task SaveBookingsAsync(List<Booking> bookings)
        {
            await _fileLock.WaitAsync();
            try
            {
                var json = JsonSerializer.Serialize(bookings, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_bookingsFilePath, json);
            }
            finally
            {
                _fileLock.Release();
            }
        }

        private void TryMigrateLegacyBookingsFile(IWebHostEnvironment environment)
        {
            try
            {
                if (File.Exists(_bookingsFilePath))
                {
                    return;
                }

                var legacyCandidates = new[]
                {
                    Path.Combine(environment.ContentRootPath, "App_Data", "bookings.json"),
                    Path.Combine(environment.WebRootPath ?? string.Empty, "bookings.json")
                };

                var source = legacyCandidates.FirstOrDefault(File.Exists);
                if (string.IsNullOrWhiteSpace(source))
                {
                    return;
                }

                File.Copy(source, _bookingsFilePath, overwrite: false);
                _logger.LogInformation("Migrated bookings file from {Source} to {Target}", source, _bookingsFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to migrate legacy bookings file to {Target}", _bookingsFilePath);
            }
        }

        private string ResolveWritableBookingsPath(IWebHostEnvironment environment, string? configuredPath)
        {
            var candidates = new List<string>();

            if (!string.IsNullOrWhiteSpace(configuredPath))
            {
                candidates.Add(configuredPath);
            }

            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (!string.IsNullOrWhiteSpace(localAppData))
            {
                candidates.Add(Path.Combine(localAppData, "MRMstudios", "bookings.json"));
            }

            candidates.Add(Path.Combine(environment.ContentRootPath, "App_Data", "bookings.json"));

            foreach (var candidate in candidates)
            {
                try
                {
                    var dir = Path.GetDirectoryName(candidate);
                    if (!string.IsNullOrWhiteSpace(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    return candidate;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Bookings path not writable: {CandidatePath}", candidate);
                }
            }

            // Last-resort relative path to avoid app startup failure.
            return Path.Combine("App_Data", "bookings.json");
        }
    }
}

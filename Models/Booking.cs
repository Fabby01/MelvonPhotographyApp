using System.ComponentModel.DataAnnotations;

namespace MRMstudios.Models
{
    public class Booking
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(30)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ServiceType { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime PreferredDate { get; set; }

        [StringLength(1000)]
        public string SpecialNotes { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled
    }
}

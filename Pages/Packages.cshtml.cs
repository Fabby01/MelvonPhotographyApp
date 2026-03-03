using Microsoft.AspNetCore.Mvc.RazorPages;
using MRMstudios.Models;
using MRMstudios.Services;

namespace MRMstudios.Pages
{
    public class PackagesModel : PageModel
    {
        private readonly IBookingService _bookingService;

        public PackagesModel(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        public List<Service> Services { get; set; } = new();

        public void OnGet()
        {
            Services = _bookingService.GetServices();
        }
    }
}

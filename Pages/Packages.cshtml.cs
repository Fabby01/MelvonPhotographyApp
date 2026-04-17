using Microsoft.AspNetCore.Mvc.RazorPages;
using MRMstudios.Models;
using MRMstudios.Services;

namespace MRMstudios.Pages
{
    public class PackagesModel : PageModel
    {
        private readonly IContentService _contentService;

        public PackagesModel(IContentService contentService)
        {
            _contentService = contentService;
        }

        public List<Service> Services { get; set; } = new();
        public string? BackgroundImageUrl { get; set; }

        public async Task OnGetAsync()
        {
            Services = await _contentService.GetServicesAsync();
            var sections = await _contentService.GetSectionPhotosAsync();
            if (sections.TryGetValue("services", out var photo) && !string.IsNullOrWhiteSpace(photo.Src))
            {
                BackgroundImageUrl = photo.Src;
            }
        }
    }
}

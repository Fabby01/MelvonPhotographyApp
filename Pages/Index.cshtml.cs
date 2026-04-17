using Microsoft.AspNetCore.Mvc.RazorPages;
using MRMstudios.Services;

namespace MRMstudios.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IContentService _contentService;

    public IndexModel(ILogger<IndexModel> logger, IContentService contentService)
    {
        _logger = logger;
        _contentService = contentService;
    }

    public string? HeroBackgroundUrl { get; set; }
    public string? AboutPhotoLeft { get; set; }
    public string? AboutPhotoRight { get; set; }

    public async Task OnGetAsync()
    {
        var sections = await _contentService.GetSectionPhotosAsync();
        if (sections.TryGetValue("hero", out var hero) && !string.IsNullOrWhiteSpace(hero.Src))
        {
            HeroBackgroundUrl = hero.Src;
        }
        if (sections.TryGetValue("about-left", out var aboutLeft) && !string.IsNullOrWhiteSpace(aboutLeft.Src))
        {
            AboutPhotoLeft = aboutLeft.Src;
        }
        if (sections.TryGetValue("about-right", out var aboutRight) && !string.IsNullOrWhiteSpace(aboutRight.Src))
        {
            AboutPhotoRight = aboutRight.Src;
        }
    }
}

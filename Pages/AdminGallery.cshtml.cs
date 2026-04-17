using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace MRMstudios.Pages
{
    public class AdminGalleryModel : PageModel
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<AdminGalleryModel> _logger;

        public AdminGalleryModel(IWebHostEnvironment webHostEnvironment, ILogger<AdminGalleryModel> logger)
        {
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        public List<GalleryImage> PortfolioImages { get; set; } = new();
        public Dictionary<string, SectionPhoto> SectionPhotos { get; set; } = new();
        public string? Message { get; set; }
        public string? Error { get; set; }

        [BindProperty]
        public string CurrentSection { get; set; } = "portfolio";

        public IActionResult OnGet()
        {
            var isAuthenticated = HttpContext.Session.GetString("AdminAuthenticated") == "true";
            if (!isAuthenticated)
            {
                return RedirectToPage("/AdminLogin");
            }

            LoadAllPhotos();
            return Page();
        }

        public async Task<IActionResult> OnPostUploadAsync(IFormFile file, string section)
        {
            var isAuthenticated = HttpContext.Session.GetString("AdminAuthenticated") == "true";
            if (!isAuthenticated)
            {
                return RedirectToPage("/AdminLogin");
            }

            if (file == null || file.Length == 0)
            {
                Error = "Please select a file to upload.";
                LoadAllPhotos();
                return Page();
            }

            try
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    Error = "Only image files (JPG, PNG, GIF, WEBP) are allowed.";
                    LoadAllPhotos();
                    return Page();
                }

                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(uploadPath)!);

                using (var stream = new FileStream(uploadPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                if (section == "portfolio")
                {
                    var imagesJsonPath = Path.Combine(_webHostEnvironment.WebRootPath, "images.json");
                    var images = LoadImagesFromJson(imagesJsonPath);
                    images.Add(new GalleryImage
                    {
                        Src = $"/images/{fileName}",
                        Caption = Path.GetFileNameWithoutExtension(file.FileName)
                    });
                    SaveImagesToJson(imagesJsonPath, images);
                    Message = "Portfolio image uploaded successfully!";
                }
                else
                {
                    var sectionsJsonPath = Path.Combine(_webHostEnvironment.WebRootPath, "sections.json");
                    var sections = LoadSectionsFromJson(sectionsJsonPath);
                    
                    if (!sections.ContainsKey(section))
                    {
                        sections[section] = new SectionPhoto();
                    }

                    sections[section] = new SectionPhoto
                    {
                        Src = $"/images/{fileName}",
                        Caption = Path.GetFileNameWithoutExtension(file.FileName),
                        UpdatedAt = DateTime.Now
                    };

                    SaveSectionsToJson(sectionsJsonPath, sections);
                    Message = $"Background photo for '{section}' updated successfully!";
                }

                _logger.LogInformation("Image uploaded by admin to section '{Section}': {FileName}", section, fileName);
            }
            catch (Exception ex)
            {
                Error = $"Error uploading file: {ex.Message}";
                _logger.LogError(ex, "Error uploading image to section {Section}", section);
            }

            LoadAllPhotos();
            return Page();
        }

        public IActionResult OnPostDelete(string imageUrl, string section)
        {
            var isAuthenticated = HttpContext.Session.GetString("AdminAuthenticated") == "true";
            if (!isAuthenticated)
            {
                return RedirectToPage("/AdminLogin");
            }

            try
            {
                if (section == "portfolio")
                {
                    var imagesJsonPath = Path.Combine(_webHostEnvironment.WebRootPath, "images.json");
                    var images = LoadImagesFromJson(imagesJsonPath);
                    images.RemoveAll(i => i.Src == imageUrl);
                    SaveImagesToJson(imagesJsonPath, images);
                }
                else
                {
                    var sectionsJsonPath = Path.Combine(_webHostEnvironment.WebRootPath, "sections.json");
                    var sections = LoadSectionsFromJson(sectionsJsonPath);
                    
                    if (sections.ContainsKey(section))
                    {
                        sections.Remove(section);
                    }

                    SaveSectionsToJson(sectionsJsonPath, sections);
                }

                var fileName = Path.GetFileName(imageUrl);
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", fileName);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                Message = "Image removed successfully!";
                _logger.LogInformation("Image deleted by admin from section '{Section}': {ImageUrl}", section, imageUrl);
            }
            catch (Exception ex)
            {
                Error = $"Error deleting image: {ex.Message}";
                _logger.LogError(ex, "Error deleting image from section {Section}", section);
            }

            LoadAllPhotos();
            return Page();
        }

        private void LoadAllPhotos()
        {
            try
            {
                // Load portfolio images
                var imagesJsonPath = Path.Combine(_webHostEnvironment.WebRootPath, "images.json");
                PortfolioImages = LoadImagesFromJson(imagesJsonPath);

                // Load section photos
                var sectionsJsonPath = Path.Combine(_webHostEnvironment.WebRootPath, "sections.json");
                SectionPhotos = LoadSectionsFromJson(sectionsJsonPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading photos");
            }
        }

        private List<GalleryImage> LoadImagesFromJson(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
                return new List<GalleryImage>();

            var json = System.IO.File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<GalleryImage>>(json) ?? new List<GalleryImage>();
        }

        private Dictionary<string, SectionPhoto> LoadSectionsFromJson(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
                return new Dictionary<string, SectionPhoto>();

            var json = System.IO.File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<Dictionary<string, SectionPhoto>>(json) ?? new Dictionary<string, SectionPhoto>();
        }

        private void SaveImagesToJson(string filePath, List<GalleryImage> images)
        {
            var json = JsonSerializer.Serialize(images, new JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(filePath, json);
        }

        private void SaveSectionsToJson(string filePath, Dictionary<string, SectionPhoto> sections)
        {
            var json = JsonSerializer.Serialize(sections, new JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(filePath, json);
        }
    }

    public class GalleryImage
    {
        public string Src { get; set; } = string.Empty;
        public string Caption { get; set; } = string.Empty;
    }

    public class SectionPhoto
    {
        public string Src { get; set; } = string.Empty;
        public string Caption { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}

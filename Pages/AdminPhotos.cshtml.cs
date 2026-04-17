using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MRMstudios.Models;
using System.Text.Json;

namespace MRMstudios.Pages
{
    public class AdminPhotosModel : PageModel
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<AdminPhotosModel> _logger;

        public AdminPhotosModel(IWebHostEnvironment webHostEnvironment, ILogger<AdminPhotosModel> logger)
        {
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        public List<GalleryImage> GalleryImages { get; set; } = new();
        public string? Message { get; set; }
        public string? Error { get; set; }

        public IActionResult OnGet()
        {
            // Check authentication
            var isAuthenticated = HttpContext.Session.GetString("AdminAuthenticated") == "true";
            if (!isAuthenticated)
            {
                return RedirectToPage("/AdminLogin");
            }

            LoadGalleryImages();
            return Page();
        }

        public async Task<IActionResult> OnPostUploadAsync(IFormFile file)
        {
            // Check authentication
            var isAuthenticated = HttpContext.Session.GetString("AdminAuthenticated") == "true";
            if (!isAuthenticated)
            {
                return RedirectToPage("/AdminLogin");
            }

            if (file == null || file.Length == 0)
            {
                Error = "Please select a file to upload.";
                LoadGalleryImages();
                return Page();
            }

            try
            {
                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    Error = "Only image files (JPG, PNG, GIF, WEBP) are allowed.";
                    LoadGalleryImages();
                    return Page();
                }

                // Create unique filename
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", fileName);

                // Ensure directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(uploadPath)!);

                // Save file
                using (var stream = new FileStream(uploadPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Add to gallery JSON
                var imagesJsonPath = Path.Combine(_webHostEnvironment.WebRootPath, "images.json");
                var images = LoadImagesFromJson(imagesJsonPath);
                images.Add(new GalleryImage
                {
                    Src = $"/images/{fileName}",
                    Caption = Path.GetFileNameWithoutExtension(file.FileName)
                });

                SaveImagesToJson(imagesJsonPath, images);

                Message = $"Image '{file.FileName}' uploaded successfully!";
                _logger.LogInformation($"Image uploaded by admin: {fileName}");
            }
            catch (Exception ex)
            {
                Error = $"Error uploading file: {ex.Message}";
                _logger.LogError(ex, "Error uploading image");
            }

            LoadGalleryImages();
            return Page();
        }

        public IActionResult OnPostDelete(string imageUrl)
        {
            // Check authentication
            var isAuthenticated = HttpContext.Session.GetString("AdminAuthenticated") == "true";
            if (!isAuthenticated)
            {
                return RedirectToPage("/AdminLogin");
            }

            try
            {
                // Remove from JSON
                var imagesJsonPath = Path.Combine(_webHostEnvironment.WebRootPath, "images.json");
                var images = LoadImagesFromJson(imagesJsonPath);
                images.RemoveAll(i => i.Src == imageUrl);
                SaveImagesToJson(imagesJsonPath, images);

                // Delete physical file if it exists
                var fileName = Path.GetFileName(imageUrl);
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", fileName);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                Message = "Image removed successfully!";
                _logger.LogInformation($"Image deleted by admin: {imageUrl}");
            }
            catch (Exception ex)
            {
                Error = $"Error deleting image: {ex.Message}";
                _logger.LogError(ex, "Error deleting image");
            }

            LoadGalleryImages();
            return Page();
        }

        private void LoadGalleryImages()
        {
            try
            {
                var imagesJsonPath = Path.Combine(_webHostEnvironment.WebRootPath, "images.json");
                GalleryImages = LoadImagesFromJson(imagesJsonPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading gallery images");
                GalleryImages = new List<GalleryImage>();
            }
        }

        private List<GalleryImage> LoadImagesFromJson(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                return new List<GalleryImage>();
            }

            var json = System.IO.File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<GalleryImage>>(json) ?? new List<GalleryImage>();
        }

        private void SaveImagesToJson(string filePath, List<GalleryImage> images)
        {
            var json = JsonSerializer.Serialize(images, new JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(filePath, json);
        }
    }
}


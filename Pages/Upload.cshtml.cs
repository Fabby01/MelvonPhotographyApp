using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace MRMstudios.Pages
{
    public class UploadModel : PageModel
    {
        [BindProperty]
        public IFormFile? Photo { get; set; }

        [BindProperty]
        public string? Caption { get; set; }

        public string? Message { get; set; }
        public string? Error { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Photo == null)
            {
                Error = "Please select a photo.";
                return Page();
            }

            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            if (!Directory.Exists(uploadsDir))
                Directory.CreateDirectory(uploadsDir);

            var fileName = Path.GetFileName(Photo.FileName);
            var filePath = Path.Combine(uploadsDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await Photo.CopyToAsync(stream);
            }

            // ✅ Update images.json
            var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images.json");
            var images = new List<ImageItem>();

            if (System.IO.File.Exists(jsonPath))
            {
                var json = await System.IO.File.ReadAllTextAsync(jsonPath);
                images = JsonSerializer.Deserialize<List<ImageItem>>(json) ?? new List<ImageItem>();
            }

            images.Add(new ImageItem
            {
                src = "/images/" + fileName,
                caption = Caption ?? ""
            });

            var newJson = JsonSerializer.Serialize(images, new JsonSerializerOptions { WriteIndented = true });
            await System.IO.File.WriteAllTextAsync(jsonPath, newJson);

            Message = "✅ Photo uploaded successfully!";
            return Page();
        }

        public class ImageItem
        {
            public string src { get; set; } = "";
            public string caption { get; set; } = "";
        }
    }
}

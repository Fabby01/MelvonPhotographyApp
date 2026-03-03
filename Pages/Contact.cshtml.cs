using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MelvonPhotographyApp.Pages
{
    public class ContactModel : PageModel
    {
        [BindProperty]
        public string? Name { get; set; }

        [BindProperty]
        public string? Email { get; set; }

        [BindProperty]
        public string? Subject { get; set; }

        [BindProperty]
        public string? Message { get; set; }

        public string? ConfirmationMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Please fill in all required fields.";
                return Page();
            }

            try
            {
                // Save contact message to a file or database
                var contactsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "contacts");
                if (!Directory.Exists(contactsDir))
                    Directory.CreateDirectory(contactsDir);

                var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                var filePath = Path.Combine(contactsDir, $"contact_{timestamp}.txt");

                var content = $@"Contact Form Submission
========================
Name: {Name}
Email: {Email}
Subject: {Subject}
Message: {Message}
Submitted: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

                await System.IO.File.WriteAllTextAsync(filePath, content);

                ConfirmationMessage = "✅ Thank you for your message! We'll get back to you shortly.";
                Name = Email = Subject = Message = null;
                return Page();
            }
            catch
            {
                ErrorMessage = "An error occurred while sending your message. Please try again.";
                return Page();
            }
        }
    }
}

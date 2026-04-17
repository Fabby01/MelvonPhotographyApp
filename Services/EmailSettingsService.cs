using MRMstudios.Models;
using System.IO;
using System.Text.Json;

namespace MRMstudios.Services
{
    public class EmailSettingsService : IEmailSettingsService
    {
        private readonly string _settingsFilePath;
        private readonly IConfiguration _configuration;

        public EmailSettingsService(IWebHostEnvironment environment, IConfiguration configuration)
        {
            _configuration = configuration;
            var appDataPath = Path.Combine(environment.ContentRootPath, "App_Data");
            Directory.CreateDirectory(appDataPath);
            _settingsFilePath = Path.Combine(appDataPath, "emailsettings.json");
        }

        public async Task<EmailSettings> GetSettingsAsync()
        {
            if (!File.Exists(_settingsFilePath))
            {
                var defaultSettings = GetDefaultSettings();
                await SaveSettingsAsync(defaultSettings);
                return defaultSettings;
            }

            var json = await File.ReadAllTextAsync(_settingsFilePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                var defaultSettings = GetDefaultSettings();
                await SaveSettingsAsync(defaultSettings);
                return defaultSettings;
            }

            return JsonSerializer.Deserialize<EmailSettings>(json) ?? GetDefaultSettings();
        }

        public async Task SaveSettingsAsync(EmailSettings settings)
        {
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_settingsFilePath, json);
        }

        private EmailSettings GetDefaultSettings()
        {
            var enableSsl = (_configuration["Email:EnableSsl"] ?? "true").ToLower() == "true";
            return new EmailSettings
            {
                SmtpHost = _configuration["Email:SmtpHost"] ?? string.Empty,
                SmtpPort = int.TryParse(_configuration["Email:SmtpPort"], out var port) ? port : 587,
                SmtpUser = _configuration["Email:SmtpUser"] ?? string.Empty,
                SmtpPass = _configuration["Email:SmtpPass"] ?? string.Empty,
                FromAddress = _configuration["Email:FromAddress"] ?? "mel.dimplz@gmail.com",
                FromName = _configuration["Email:FromName"] ?? "MRMstudios",
                EnableSsl = enableSsl,
                OwnerEmail = _configuration["Email:OwnerEmail"] ?? "mel.dimplz@gmail.com"
            };
        }
    }
}

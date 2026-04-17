using MRMstudios.Models;

namespace MRMstudios.Services
{
    public interface IEmailSettingsService
    {
        Task<EmailSettings> GetSettingsAsync();
        Task SaveSettingsAsync(EmailSettings settings);
    }
}

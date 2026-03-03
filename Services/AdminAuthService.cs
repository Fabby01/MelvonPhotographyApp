using System.Security.Cryptography;
using System.Text.Json;

namespace MRMstudios.Services
{
    public interface IAdminAuthService
    {
        Task<bool> ValidateCredentialsAsync(string username, string password);
        Task<bool> GenerateAndEmailNewPasswordAsync();
    }

    public class AdminAuthService : IAdminAuthService
    {
        private const string AdminUsername = "admin";
        private static readonly string[] AdminEmails =
        {
            "mel.dimplz@gmail.com",
            "fabiana.mkova2001@gmail.com"
        };
        private readonly string _authFilePath;
        private readonly IEmailService _emailService;
        private readonly ILogger<AdminAuthService> _logger;
        private readonly SemaphoreSlim _fileLock = new(1, 1);

        public AdminAuthService(
            IWebHostEnvironment environment,
            IEmailService emailService,
            ILogger<AdminAuthService> logger)
        {
            _emailService = emailService;
            _logger = logger;

            var authDir = Path.Combine(environment.ContentRootPath, "App_Data");
            Directory.CreateDirectory(authDir);
            _authFilePath = Path.Combine(authDir, "admin-auth.json");
        }

        public async Task<bool> ValidateCredentialsAsync(string username, string password)
        {
            if (!string.Equals(username, AdminUsername, StringComparison.Ordinal))
            {
                return false;
            }

            var state = await ReadOrInitializeStateAsync();
            return VerifyPassword(password, state.PasswordHash, state.Salt);
        }

        public async Task<bool> GenerateAndEmailNewPasswordAsync()
        {
            var newPassword = GenerateSecurePassword();
            var salt = GenerateSalt();
            var hash = HashPassword(newPassword, salt);

            await _fileLock.WaitAsync();
            try
            {
                var state = new AdminAuthState
                {
                    Username = AdminUsername,
                    PasswordHash = hash,
                    Salt = salt,
                    UpdatedAtUtc = DateTime.UtcNow
                };

                var json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_authFilePath, json);
            }
            finally
            {
                _fileLock.Release();
            }

            var successfulSends = 0;
            foreach (var email in AdminEmails)
            {
                var sent = await _emailService.SendAdminPasswordResetAsync(email, newPassword);
                if (sent)
                {
                    successfulSends++;
                }
                else
                {
                    _logger.LogError("Generated admin password, but failed to send reset email to {Email}.", email);
                }
            }

            return successfulSends > 0;
        }

        private async Task<AdminAuthState> ReadOrInitializeStateAsync()
        {
            await _fileLock.WaitAsync();
            try
            {
                if (!File.Exists(_authFilePath))
                {
                    var salt = GenerateSalt();
                    var hash = HashPassword(GenerateSecurePassword(), salt);
                    var initial = new AdminAuthState
                    {
                        Username = AdminUsername,
                        PasswordHash = hash,
                        Salt = salt,
                        UpdatedAtUtc = DateTime.UtcNow
                    };

                    var initialJson = JsonSerializer.Serialize(initial, new JsonSerializerOptions { WriteIndented = true });
                    await File.WriteAllTextAsync(_authFilePath, initialJson);
                    _logger.LogWarning("Admin credentials initialized. Use password generation from login page to get a usable password by email.");
                    return initial;
                }

                var json = await File.ReadAllTextAsync(_authFilePath);
                var state = JsonSerializer.Deserialize<AdminAuthState>(json);
                if (state == null || string.IsNullOrWhiteSpace(state.PasswordHash) || string.IsNullOrWhiteSpace(state.Salt))
                {
                    throw new InvalidOperationException("Invalid admin auth file.");
                }

                return state;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read admin auth state.");
                throw;
            }
            finally
            {
                _fileLock.Release();
            }
        }

        private static string GenerateSalt()
        {
            var salt = RandomNumberGenerator.GetBytes(16);
            return Convert.ToBase64String(salt);
        }

        private static string HashPassword(string password, string saltBase64)
        {
            var salt = Convert.FromBase64String(saltBase64);
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
            return Convert.ToBase64String(hash);
        }

        private static bool VerifyPassword(string password, string expectedHashBase64, string saltBase64)
        {
            var actual = HashPassword(password, saltBase64);
            return CryptographicOperations.FixedTimeEquals(
                Convert.FromBase64String(actual),
                Convert.FromBase64String(expectedHashBase64));
        }

        private static string GenerateSecurePassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@#$%*?";
            var bytes = RandomNumberGenerator.GetBytes(16);
            var result = new char[16];
            for (var i = 0; i < result.Length; i++)
            {
                result[i] = chars[bytes[i] % chars.Length];
            }
            return new string(result);
        }

        private class AdminAuthState
        {
            public string Username { get; set; } = AdminUsername;
            public string PasswordHash { get; set; } = string.Empty;
            public string Salt { get; set; } = string.Empty;
            public DateTime UpdatedAtUtc { get; set; }
        }
    }
}

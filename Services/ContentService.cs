using MRMstudios.Models;
using System.Text.Json;

namespace MRMstudios.Services
{
    public class ContentService : IContentService
    {
        private readonly string _servicesFilePath;
        private readonly string _sectionsFilePath;
        private readonly ILogger<ContentService> _logger;
        private readonly SemaphoreSlim _fileLock = new(1, 1);

        public ContentService(IWebHostEnvironment environment, ILogger<ContentService> logger)
        {
            _logger = logger;
            var appDataPath = Path.Combine(environment.ContentRootPath, "App_Data");
            Directory.CreateDirectory(appDataPath);
            _servicesFilePath = Path.Combine(appDataPath, "services.json");
            _sectionsFilePath = Path.Combine(appDataPath, "sections.json");
        }

        public async Task<List<Service>> GetServicesAsync()
        {
            await _fileLock.WaitAsync();
            try
            {
                if (!File.Exists(_servicesFilePath))
                {
                    var defaultServices = GetDefaultServices();
                    await SaveServicesInternalAsync(defaultServices);
                    return defaultServices;
                }

                var json = await File.ReadAllTextAsync(_servicesFilePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    var defaultServices = GetDefaultServices();
                    await SaveServicesInternalAsync(defaultServices);
                    return defaultServices;
                }

                return JsonSerializer.Deserialize<List<Service>>(json) ?? GetDefaultServices();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load services from {_servicesFilePath}", _servicesFilePath);
                return GetDefaultServices();
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task SaveServicesAsync(List<Service> services)
        {
            await _fileLock.WaitAsync();
            try
            {
                await SaveServicesInternalAsync(services);
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task<Dictionary<string, SectionPhoto>> GetSectionPhotosAsync()
        {
            await _fileLock.WaitAsync();
            try
            {
                if (!File.Exists(_sectionsFilePath))
                {
                    var emptySections = new Dictionary<string, SectionPhoto>();
                    await SaveSectionsInternalAsync(emptySections);
                    return emptySections;
                }

                var json = await File.ReadAllTextAsync(_sectionsFilePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    var emptySections = new Dictionary<string, SectionPhoto>();
                    await SaveSectionsInternalAsync(emptySections);
                    return emptySections;
                }

                return JsonSerializer.Deserialize<Dictionary<string, SectionPhoto>>(json) ?? new Dictionary<string, SectionPhoto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load section photos from {_sectionsFilePath}", _sectionsFilePath);
                return new Dictionary<string, SectionPhoto>();
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task SaveSectionPhotosAsync(Dictionary<string, SectionPhoto> sections)
        {
            await _fileLock.WaitAsync();
            try
            {
                await SaveSectionsInternalAsync(sections);
            }
            finally
            {
                _fileLock.Release();
            }
        }

        private Task SaveServicesInternalAsync(List<Service> services)
        {
            var json = JsonSerializer.Serialize(services, new JsonSerializerOptions { WriteIndented = true });
            return File.WriteAllTextAsync(_servicesFilePath, json);
        }

        private Task SaveSectionsInternalAsync(Dictionary<string, SectionPhoto> sections)
        {
            var json = JsonSerializer.Serialize(sections, new JsonSerializerOptions { WriteIndented = true });
            return File.WriteAllTextAsync(_sectionsFilePath, json);
        }

        private static List<Service> GetDefaultServices()
        {
            return new List<Service>
            {
                new Service { Id = 1, Name = "Wedding Photography", Price = 800, Description = "Full day wedding coverage with multiple locations" },
                new Service { Id = 2, Name = "Portrait Session", Price = 300, Description = "Professional portrait session with retouching" },
                new Service { Id = 3, Name = "Corporate Photography", Price = 500, Description = "Corporate event or headshot photography" },
                new Service { Id = 4, Name = "Event Photography", Price = 600, Description = "Birthday, anniversary, or special event coverage" }
            };
        }
    }
}

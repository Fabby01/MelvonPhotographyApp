using MRMstudios.Models;

namespace MRMstudios.Services
{
    public interface IContentService
    {
        Task<List<Service>> GetServicesAsync();
        Task SaveServicesAsync(List<Service> services);
        Task<Dictionary<string, SectionPhoto>> GetSectionPhotosAsync();
        Task SaveSectionPhotosAsync(Dictionary<string, SectionPhoto> sections);
    }
}

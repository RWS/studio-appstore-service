using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.Common.Interface
{
    public interface INamesRepository
    {
        Task<IEnumerable<NameMapping>> GetAllNameMappings(List<string> pluginsNames);
        Task<IEnumerable<NameMapping>> GetAllNameMappings();
        Task UpdateMappings(List<NameMapping> namesMapping);
        Task DeleteMapping(string id);
    }
}

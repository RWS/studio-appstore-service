using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface INamesRepository
    {
        Task<IEnumerable<NameMapping>> GetAllNameMappings(List<string> pluginsNames);
        Task<IEnumerable<NameMapping>> GetAllNameMappings();
        Task<bool> TryUpdateMapping(NameMapping mapping);
        Task DeleteMapping(string id);
    }
}

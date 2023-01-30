using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface INamesRepository
    {
        Task<IEnumerable<NameMapping>> GetAllNames(List<string> pluginsNames);
        Task<IEnumerable<NameMapping>> GetAllNames();
        Task<bool> TryUpdateMapping(NameMapping mapping);
        Task DeleteMapping(string id);
    }
}

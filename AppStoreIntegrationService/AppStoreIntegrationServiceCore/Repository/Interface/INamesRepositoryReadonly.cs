using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface INamesRepositoryReadonly
    {
        Task<IEnumerable<NameMapping>> GetAllNames(List<string> pluginsNames);
        Task<IEnumerable<NameMapping>> GetAllNames();
    }
}

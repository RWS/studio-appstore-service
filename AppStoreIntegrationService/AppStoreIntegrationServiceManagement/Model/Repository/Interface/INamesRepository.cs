using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface INamesRepository : INamesRepositoryReadonly
    {
        Task<bool> TryUpdateMapping(NameMapping mapping);
        Task DeleteMapping(string id);
    }
}

using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;

namespace AppStoreIntegrationServiceManagement.Repository.Interface
{
    public interface INamesRepository : INamesRepositoryReadonly
    {
        Task<bool> TryUpdateMapping(NameMapping mapping);
        Task DeleteMapping(string id);
    }
}

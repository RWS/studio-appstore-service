using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.V1.Interface
{
    public interface IAzureRepository<T>
    {
        Task<List<T>> GetPluginsListFromContainer();
    }
}

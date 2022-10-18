using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.V1.Interface
{
    public interface IAzureRepository<T>
    {
        Task<List<NameMapping>> GetNameMappingsFromContainer();
        Task<List<T>> GetPluginsListFromContainer();
        Task UploadToContainer(Stream pluginsStream);
    }
}

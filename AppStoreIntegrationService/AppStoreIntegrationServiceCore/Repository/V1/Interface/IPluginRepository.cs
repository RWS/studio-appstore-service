using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.V1.Interface
{
    public interface IPluginRepository<T>
    {
        List<T> SearchPlugins(List<T> pluginsList, PluginFilter filter);

        Task<List<T>> GetAll(string sortOrder);

        Task<List<CategoryDetails>> GetCategories();
    }
}

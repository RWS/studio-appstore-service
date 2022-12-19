using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface ILocalRepository
    {
        Task<List<PluginDetails<PluginVersion<string>, string>>> ReadPluginsFromFile();
        Task<PluginResponse<PluginDetails<PluginVersion<string>, string>>> ReadFromFile();
        Task SavePluginsToFile(List<PluginDetails<PluginVersion<string>, string>> plugins);
        Task SaveProductsToFile(List<ProductDetails> products);
        Task SaveParentsToFile(List<ParentProduct> products);
        Task SaveMappingsToFile(List<NameMapping> names);
        Task SaveCategoriesToFile(List<CategoryDetails> categories);
        Task SaveAPIVersionToFile(string version);
        Task<List<ProductDetails>> ReadProductsFromFile();
        Task<List<ParentProduct>> ReadParentsFromFile();
        Task<List<NameMapping>> ReadMappingsFromFile();
        Task<List<CategoryDetails>> ReadCategoriesFromFile();
        Task<string> GetAPIVersionFromFile();
    }
}

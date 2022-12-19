using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface IAzureRepository
    {
        Task<List<PluginDetails<PluginVersion<string>, string>>> GetPluginsFromContainer();
        Task<PluginResponse<PluginDetails<PluginVersion<string>, string>>> ReadFromContainer();
        Task<List<NameMapping>> GetNameMappingsFromContainer();
        Task<List<ParentProduct>> GetParentProductsFromContainer();
        Task<List<ProductDetails>> GetProductsFromContainer();
        Task<List<CategoryDetails>> GetCategoriesFromContainer();
        Task<SiteSettings> GetSettingsFromContainer();
        Task<string> GetAPIVersionFromContainer();
        Task UpdateMappingsFileBlob(List<NameMapping> mappings);
        Task UpdateSettingsFileBlob(SiteSettings settings);
        Task UpdatePluginsFileBlob(List<PluginDetails<PluginVersion<string>, string>> plugins);
        Task UpdateProductsFileBlob(List<ProductDetails> products);
        Task UpdateParentsFileBlob(List<ParentProduct> products);
        Task UpdateCategoriesFileBlob(List<CategoryDetails> products);
        Task UpdateAPIVersion(string version);
        Task BackupFile(List<PluginDetails<PluginVersion<string>, string>> plugins);
    }
}

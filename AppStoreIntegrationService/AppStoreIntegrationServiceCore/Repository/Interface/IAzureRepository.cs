using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface IAzureRepository
    {
        /// <summary>
        /// Refeshes the list of plugins from public appstore service
        /// </summary>
        Task UploadToContainer(Stream pluginsStream);
        Task<List<PluginDetails>> GetPluginsListFromContainer();
        Task<List<NameMapping>> GetNameMappingsFromContainer();
        Task<List<SupportedProductDetails>> GetProductsFromContainer();
        Task<SiteSettings> GetSettingsFromContainer();

        /// <summary>
        /// String which contains the plugins list updated. (Format of the file is json)
        /// </summary>
        Task UpdatePluginsFileBlob(string fileContent);
        Task UpdateNameMappingsFileBlob(string fileContent);
        Task UpdateProductsFileBlob(string fileContent);
        Task UpdateSettingsFileBlob(string fileContent);

        /// <summary>
        /// Backup plugins list
        /// </summary>
        Task BackupFile(string fileContent);
    }
}

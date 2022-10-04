using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface IAzureRepository
    {
        /// <summary>
        /// Refeshes the list of plugins from public appstore service
        /// </summary>
        public Task UploadToContainer(Stream pluginsStream);
        public Task<List<PluginDetails>> GetPluginsListFromContainer();
        public Task<List<NameMapping>> GetNameMappingsFromContainer();
        public Task<List<SupportedProductDetails>> GetProductsFromContainer();
        /// <summary>
        /// String which contains the plugins list updated. (Format of the file is json)
        /// </summary>
        public Task UpdatePluginsFileBlob(string fileContent);

        public Task UpdateNameMappingsFileBlob(string fileContent);

        public Task UpdateProductsFileBlob(string fileContent);

        /// <summary>
        /// Backup plugins list
        /// </summary>
        public Task BackupFile(string fileContent);
    }
}

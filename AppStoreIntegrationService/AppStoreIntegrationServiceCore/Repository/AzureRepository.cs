using AppStoreIntegrationServiceCore.Model;
using Newtonsoft.Json;
using AppStoreIntegrationServiceCore.Repository.Interface;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Text;
using Azure;

namespace AppStoreIntegrationServiceCore.Repository
{
    public class AzureRepository : IResponseManager, ISettingsManager
    {
        private readonly IConfigurationSettings _configurationSettings;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobContainerClient _container;
        private readonly BlobClient _pluginsBlob;
        private readonly BlobClient _pluginsBlobBackup;
        private readonly BlobClient _mappingsBlob;
        private readonly BlobClient _settingsBlob;

        public AzureRepository(IConfigurationSettings configurationSettings)
        {
            _configurationSettings = configurationSettings;
            _blobServiceClient = new BlobServiceClient(new Uri($"https://{_configurationSettings.StorageAccountName}.blob.core.windows.net"), new DefaultAzureCredential());
            _container = _blobServiceClient.GetBlobContainerClient(_configurationSettings.BlobName);

            CreateIfNotExists(_container, _configurationSettings.PluginsFileName);
            CreateIfNotExists(_container, _configurationSettings.PluginsFileBackUpPath);
            CreateIfNotExists(_container, _configurationSettings.MappingFileName);
            CreateIfNotExists(_container, _configurationSettings.SettingsFileName);
        }

        public async Task<List<T>> GetPluginsFromContainer()
        {
            string containerContent = await _pluginsBlockBlob.DownloadTextAsync(Encoding.UTF8, null, _blobRequestOptions, null);

        public async Task<PluginResponse<T>> ReadFromContainer()
        {
            if (_configurationSettings.PluginsFileName == null)
            {
                return new PluginResponse<T>();
            }

            var content = await _pluginsBlob.DownloadContentAsync();
            return JsonConvert.DeserializeObject<PluginResponse<T>>(content.Value.Content.ToString()) ?? new PluginResponse<T>();
        }

        public async Task<List<ProductDetails>> GetProductsFromContainer()
        {
            return (await ReadFromContainer())?.Products;
        }

        public async Task SaveResponse(PluginResponse<PluginDetails> response)
        {
            var response = await ReadFromContainer();
            response.Value = plugins;
            var content = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
            await _pluginsBlob.UploadAsync(content, new BlobUploadOptions());
        }

        public async Task<SiteSettings> ReadSettings()
        {
            var response = await ReadFromContainer();
            response.Value = plugins;
            var content = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
            await _pluginsBlobBackup.UploadAsync(content, new BlobUploadOptions());
        }

        public async Task UpdateProductsFileBlob(List<ProductDetails> products)
        {
            var response = await ReadFromContainer();
            response.Products = products;
            var content = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
            await _pluginsBlob.UploadAsync(content, new BlobUploadOptions());
        }

        public async Task UpdateParentsFileBlob(List<ParentProduct> products)
        {
            var response = await ReadFromContainer();
            response.ParentProducts = products;
            var content = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
            await _pluginsBlob.UploadAsync(content, new BlobUploadOptions());
        }

        private CloudStorageAccount GetCloudStorageAccount()
        {
            var content = await _mappingsBlob.DownloadContentAsync();
            return JsonConvert.DeserializeObject<List<NameMapping>>(content.Value.Content.ToString()) ?? new List<NameMapping>();
        }

        public async Task<List<ParentProduct>> GetParentProductsFromContainer()
        {
            return (await ReadFromContainer())?.ParentProducts;
        }

        public async Task UpdateMappingsFileBlob(List<NameMapping> mappings)
        {
            var content = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(mappings)));
            await _mappingsBlob.UploadAsync(content, new BlobUploadOptions());
        }

        public async Task<SiteSettings> GetSettingsFromContainer()
        {
            var content = await _settingsBlob.DownloadContentAsync();
            return JsonConvert.DeserializeObject<SiteSettings>(content.Value.Content.ToString()) ?? new SiteSettings();
        }

        public async Task UpdateSettingsFileBlob(SiteSettings settings)
        {
            var content = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(settings)));
            await _settingsBlob.UploadAsync(content, new BlobUploadOptions());
        }

        public async Task<string> GetAPIVersionFromContainer()
        {
            return (await ReadFromContainer())?.APIVersion ?? "1.0.0";
        }

        private void CreateContainer(CloudStorageAccount cloudStorageAccount)
        {
            var blobName = NormalizeBlobName();
            _cloudBlobContainer = cloudStorageAccount.CreateCloudBlobClient().GetContainerReference(blobName);

        public async Task UpdateCategoriesFileBlob(List<CategoryDetails> categories)
        {
            var response = await ReadFromContainer();
            response.Categories = categories;
            var content = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
            await _pluginsBlob.UploadAsync(content, new BlobUploadOptions());
        }

        public async Task UpdateAPIVersion(string version)
        {
            var response = await ReadFromContainer();
            response.APIVersion = version;
            var content = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
            await _pluginsBlob.UploadAsync(content, new BlobUploadOptions());
        }

        private static void CreateIfNotExists(BlobContainerClient container, string fileName)
        {
            var blob = container.GetBlobClient(fileName);

            if (blob.Exists())
            {
                return;
            }

            container.UploadBlob(fileName, new MemoryStream());
        }
    }
}

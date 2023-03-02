using Azure.Identity;
using Azure.Storage.Blobs;
using AppStoreIntegrationServiceCore.Repository.Interface;
using AppStoreIntegrationServiceCore.Model;
using Newtonsoft.Json;

namespace AppStoreIntegrationServiceAPI.Model.Repository
{
    public class AzureRepositoryBase : IResponseManagerBase
    {
        protected readonly IConfigurationSettings _configurationSettings;
        protected readonly BlobContainerClient _container;
        protected readonly BlobClient _pluginsBlob;
        private readonly BlobServiceClient _blobServiceClient;

        public AzureRepositoryBase(IConfigurationSettings configurationSettings)
        {
            _configurationSettings = configurationSettings;
            _blobServiceClient = new BlobServiceClient(new Uri($"https://{_configurationSettings.StorageAccountName}.blob.core.windows.net"), new DefaultAzureCredential());
            _container = _blobServiceClient.GetBlobContainerClient(_configurationSettings.BlobName);

            _pluginsBlob = CreateIfNotExists(_container, _configurationSettings.PluginsFileName);
        }

        public async Task<PluginResponseBase<PluginDetails>> GetBaseResponse()
        {
            if (_configurationSettings.PluginsFileName == null)
            {
                return new PluginResponseBase<PluginDetails>();
            }

            var content = await _pluginsBlob.DownloadContentAsync();
            return JsonConvert.DeserializeObject<PluginResponseBase<PluginDetails>>(content.Value.Content.ToString()) ?? new PluginResponseBase<PluginDetails>();
        }

        protected static BlobClient CreateIfNotExists(BlobContainerClient container, string fileName)
        {
            var blob = container.GetBlobClient(fileName);

            if (blob.Exists())
            {
                return blob;
            }

            container.UploadBlob(fileName, new MemoryStream());
            return container.GetBlobClient(fileName);
        }
    }
}

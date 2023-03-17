using Azure.Identity;
using Azure.Storage.Blobs;
using AppStoreIntegrationServiceCore.Repository.Interface;
using AppStoreIntegrationServiceCore.Model;
using Newtonsoft.Json;
using Azure.Storage.Blobs.Models;
using System.Text;

namespace AppStoreIntegrationServiceAPI.Model.Repository
{
    public class AzureRepositoryBase : IResponseManager
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

        public async Task<PluginResponse<PluginDetails>> GetResponse()
        {
            if (_configurationSettings.PluginsFileName == null)
            {
                return new PluginResponse<PluginDetails>();
            }

            var content = await _pluginsBlob.DownloadContentAsync();
            return JsonConvert.DeserializeObject<PluginResponse<PluginDetails>>(content.Value.Content.ToString()) ?? new PluginResponse<PluginDetails>();
        }

        public async Task SaveResponse(PluginResponse<PluginDetails> response)
        {
            var content = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
            await _pluginsBlob.UploadAsync(content, new BlobUploadOptions());
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

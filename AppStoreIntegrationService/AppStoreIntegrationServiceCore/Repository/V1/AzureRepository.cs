using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Common;
using AppStoreIntegrationServiceCore.Repository.Common.Interface;
using AppStoreIntegrationServiceCore.Repository.V1.Interface;
using Microsoft.Azure.Storage.Blob;
using Newtonsoft.Json;
using System.Text;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceCore.Repository.V1
{
    public class AzureRepository<T> : AzureRepositoryBase<T>, IAzureRepository<T> where T : PluginDetails<PluginVersion<ProductDetails>, CategoryDetails>
    {
        private CloudBlockBlob _pluginsListBlockBlob;

        public AzureRepository(IConfigurationSettings configurationSettings) : base(configurationSettings)
        {
            if (configurationSettings.DeployMode != DeployMode.AzureBlob)
            {
                return;
            }

            SetCloudBlockBlobs();
            InitializeBlockBlobs();
        }

        public async Task<List<T>> GetPluginsFromContainer()
        {
            string containerContent = await _pluginsListBlockBlob.DownloadTextAsync(Encoding.UTF8, null, _blobRequestOptions, null);
            return JsonConvert.DeserializeObject<PluginResponse<T>>(containerContent)?.Value;
        }

        private void InitializeBlockBlobs()
        {
            CreateEmptyFile(_pluginsListBlockBlob);
        }

        private void SetCloudBlockBlobs()
        {
            if (!string.IsNullOrEmpty(_configurationSettings.PluginsFileNameV1))
            {
                _pluginsListBlockBlob = GetBlockBlobReference(_configurationSettings.PluginsFileNameV1);
            }
        }
    }
}

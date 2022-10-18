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
    public class AzureRepository<T> : AzureRepositoryBase<T>, IAzureRepository<T> where T : PluginDetails<PluginVersion<ProductDetails>>
    {
        private CloudBlockBlob _pluginsListBlockBlob;
        private CloudBlockBlob _pluginsBackupBlockBlob;

        public AzureRepository(IConfigurationSettings configurationSettings) : base(configurationSettings)
        {
            if (configurationSettings.DeployMode != DeployMode.AzureBlob)
            {
                return;
            }

            SetCloudBlockBlobs();
            InitializeBlockBlobs();
        }

        public async Task<List<T>> GetPluginsListFromContainer()
        {
            string containerContent = await _pluginsListBlockBlob.DownloadTextAsync(Encoding.UTF8, null, _blobRequestOptions, null);
            return JsonConvert.DeserializeObject<PluginResponse<T>>(containerContent)?.Value.Cast<T>().ToList() ?? new List<T>();
        }

        public async Task UploadToContainer(Stream pluginsStream)
        {
            await _pluginsListBlockBlob.UploadFromStreamAsync(pluginsStream, null, _blobRequestOptions, null);
        }

        protected override void InitializeBlockBlobs()
        {
            CreateEmptyFile(_pluginsListBlockBlob);
            CreateEmptyFile(_pluginsBackupBlockBlob);
        }

        protected override void SetCloudBlockBlobs()
        {
            if (!string.IsNullOrEmpty(_configurationSettings.PluginsFileNameV1))
            {
                _pluginsListBlockBlob = GetBlockBlobReference(_configurationSettings.PluginsFileNameV1);

                var backupFileName = $"{Path.GetFileNameWithoutExtension(_configurationSettings.PluginsFileNameV1)}_backupFile.json";
                _pluginsBackupBlockBlob = GetBlockBlobReference(backupFileName);
            }
        }
    }
}

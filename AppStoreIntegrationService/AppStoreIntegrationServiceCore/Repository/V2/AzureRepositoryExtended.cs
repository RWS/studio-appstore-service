using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Common;
using AppStoreIntegrationServiceCore.Repository.Common.Interface;
using AppStoreIntegrationServiceCore.Repository.V2.Interface;
using Microsoft.Azure.Storage.Blob;
using Newtonsoft.Json;
using System.Text;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceCore.Repository.V2
{
    public class AzureRepositoryExtended<T> : AzureRepositoryBase<T>, IAzureRepositoryExtended<T> where T : PluginDetails<PluginVersion<string>>
    {
        private CloudBlockBlob _pluginsListBlockBlobOptimized;
        private CloudBlockBlob _pluginsBackupBlockBlobOptimized;

        public AzureRepositoryExtended(IConfigurationSettings configurationSettings) : base(configurationSettings) 
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
            string containerContent = await _pluginsListBlockBlobOptimized.DownloadTextAsync(Encoding.UTF8, null, _blobRequestOptions, null);
            return JsonConvert.DeserializeObject<PluginResponse<T>>(containerContent)?.Value ?? new List<T>();
        }

        public async Task<List<ProductDetails>> GetProductsFromContainer()
        {
            string containerContent = await _pluginsListBlockBlobOptimized.DownloadTextAsync(Encoding.UTF8, null, _blobRequestOptions, null);
            return JsonConvert.DeserializeObject<PluginResponse<T>>(containerContent).Products ?? new List<ProductDetails>();
        }

        public async Task UploadToContainer(Stream pluginsStream)
        {
            await _pluginsListBlockBlobOptimized.UploadFromStreamAsync(pluginsStream, null, _blobRequestOptions, null);
        }

        public async Task UpdatePluginsFileBlob(string fileContent)
        {
            await _pluginsListBlockBlobOptimized.UploadTextAsync(fileContent);
        }

        public async Task BackupFile(string fileContent)
        {
            await _pluginsBackupBlockBlobOptimized.UploadTextAsync(fileContent);
        }

        public async Task UpdateProductsFileBlob(string fileContent)
        {
            await UpdatePluginsFileBlob(fileContent);
        }

        protected override void InitializeBlockBlobs()
        {
            CreateEmptyFile(_pluginsListBlockBlobOptimized);
            CreateEmptyFile(_pluginsBackupBlockBlobOptimized);
        }

        protected override void SetCloudBlockBlobs()
        {
            if (!string.IsNullOrEmpty(_configurationSettings.PluginsFileNameV2))
            {
                _pluginsListBlockBlobOptimized = GetBlockBlobReference(_configurationSettings.PluginsFileNameV2);

                var backupFileName = $"{Path.GetFileNameWithoutExtension(_configurationSettings.PluginsFileNameV2)}_backupFile.json";
                _pluginsBackupBlockBlobOptimized = GetBlockBlobReference(backupFileName);
            }
        }

        public async Task<List<ParentProduct>> GetParentProductsFromContainer()
        {
            var containterContent = await _pluginsListBlockBlobOptimized.DownloadTextAsync(Encoding.UTF8, null, _blobRequestOptions, null);
            var parents = JsonConvert.DeserializeObject<PluginResponse<T>>(containterContent)?.ParentProducts;
            return parents ?? new List<ParentProduct>();
        }
    }
}

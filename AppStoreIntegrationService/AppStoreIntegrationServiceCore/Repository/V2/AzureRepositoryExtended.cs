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
    public class AzureRepositoryExtended<T> : AzureRepositoryBase<T>, IAzureRepositoryExtended<T> where T : PluginDetails<PluginVersion<string>, string>
    {
        private CloudBlockBlob _pluginsListBlockBlobOptimized;
        private CloudBlockBlob _pluginsBackupBlockBlobOptimized;
        private CloudBlockBlob _nameMappingsBlockBlob;
        private CloudBlockBlob _settingsBlockBlob;

        public AzureRepositoryExtended(IConfigurationSettings configurationSettings) : base(configurationSettings)
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
            return (await ReadFromContainer()).Value ?? new List<T>();
        }

        private async Task<PluginResponse<T>> ReadFromContainer()
        {
            string containerContent = await _pluginsListBlockBlobOptimized.DownloadTextAsync(Encoding.UTF8, null, _blobRequestOptions, null);
            return JsonConvert.DeserializeObject<PluginResponse<T>>(containerContent) ?? new PluginResponse<T>();
        }

        public async Task<List<ProductDetails>> GetProductsFromContainer()
        {
            return (await ReadFromContainer()).Products;
        }

        public async Task UpdatePluginsFileBlob(List<T> plugins)
        {
            var response = await ReadFromContainer();
            string text = JsonConvert.SerializeObject(new PluginResponse<T>
            {
                APIVersion = response.APIVersion,
                Value = plugins,
                Products = response.Products,
                ParentProducts = response.ParentProducts,
                Categories = response.Categories
            });
            await _pluginsListBlockBlobOptimized.UploadTextAsync(text);
        }

        public async Task BackupFile(List<T> plugins)
        {
            var response = await ReadFromContainer();
            string text = JsonConvert.SerializeObject(new PluginResponse<T>
            {
                APIVersion = response.APIVersion,
                Value = plugins,
                Products = response.Products,
                ParentProducts = response.ParentProducts,
                Categories = response.Categories
            });
            await _pluginsBackupBlockBlobOptimized.UploadTextAsync(text);
        }

        public async Task UpdateProductsFileBlob(List<ProductDetails> products)
        {
            var response = await ReadFromContainer();
            string text = JsonConvert.SerializeObject(new PluginResponse<T>
            {
                APIVersion = response.APIVersion,
                Value = response.Value,
                Products = products,
                ParentProducts = response.ParentProducts,
                Categories = response.Categories
            });
            await _pluginsListBlockBlobOptimized.UploadTextAsync(text);
        }

        public async Task UpdateParentsFileBlob(List<ParentProduct> products)
        {
            var response = await ReadFromContainer();
            string text = JsonConvert.SerializeObject(new PluginResponse<T>
            {
                APIVersion = response.APIVersion,
                Value = response.Value,
                Products = response.Products,
                ParentProducts = products,
                Categories = response.Categories
            });
            await _pluginsListBlockBlobOptimized.UploadTextAsync(text);
        }

        public async Task<List<NameMapping>> GetNameMappingsFromContainer()
        {
            var containterContent = await _nameMappingsBlockBlob.DownloadTextAsync(Encoding.UTF8, null, _blobRequestOptions, null);
            return JsonConvert.DeserializeObject<List<NameMapping>>(containterContent) ?? new List<NameMapping>();
        }

        private void InitializeBlockBlobs()
        {
            CreateEmptyFile(_pluginsListBlockBlobOptimized);
            CreateEmptyFile(_pluginsBackupBlockBlobOptimized);
            CreateEmptyFile(_nameMappingsBlockBlob);
            CreateEmptyFile(_settingsBlockBlob);
        }

        private void SetCloudBlockBlobs()
        {
            if (!string.IsNullOrEmpty(_configurationSettings.PluginsFileNameV2))
            {
                _pluginsListBlockBlobOptimized = GetBlockBlobReference(_configurationSettings.PluginsFileNameV2);

                var backupFileName = $"{Path.GetFileNameWithoutExtension(_configurationSettings.PluginsFileNameV2)}_backupFile.json";
                _pluginsBackupBlockBlobOptimized = GetBlockBlobReference(backupFileName);
            }

            if (!string.IsNullOrEmpty(_configurationSettings.MappingFileName))
            {
                _nameMappingsBlockBlob = GetBlockBlobReference(_configurationSettings.MappingFileName);
            }

            if (!string.IsNullOrEmpty(_configurationSettings.SettingsFileName))
            {
                _settingsBlockBlob = GetBlockBlobReference(_configurationSettings.SettingsFileName);
            }
        }

        public async Task<List<ParentProduct>> GetParentProductsFromContainer()
        {
            return (await ReadFromContainer()).ParentProducts;
        }

        public async Task UpdateMappingsFileBlob(List<NameMapping> mappings)
        {
            var text = JsonConvert.SerializeObject(mappings);
            await _nameMappingsBlockBlob.UploadTextAsync(text);
        }

        public async Task<SiteSettings> GetSettingsFromContainer()
        {
            var containterContent = await _settingsBlockBlob.DownloadTextAsync(Encoding.UTF8, null, _blobRequestOptions, null);
            return JsonConvert.DeserializeObject<SiteSettings>(containterContent) ?? new SiteSettings();
        }

        public async Task UpdateSettingsFileBlob(SiteSettings settings)
        {
            var text = JsonConvert.SerializeObject(settings);
            await _settingsBlockBlob.UploadTextAsync(text);
        }

        public async Task<string> GetAPIVersionFromContainer()
        {
            return (await ReadFromContainer()).APIVersion ?? "1.0.0";
        }

        public async Task<List<CategoryDetails>> GetCategoriesFromContainer()
        {
            return (await ReadFromContainer()).Categories;
        }

        public async Task UpdateCategoriesFileBlob(List<CategoryDetails> categories)
        {
            var response = await ReadFromContainer();
            string text = JsonConvert.SerializeObject(new PluginResponse<T>
            {
                APIVersion = response.APIVersion,
                Value = response.Value,
                Products = response.Products,
                ParentProducts = response.ParentProducts,
                Categories = categories
            });
            await _pluginsListBlockBlobOptimized.UploadTextAsync(text);
        }
    }
}

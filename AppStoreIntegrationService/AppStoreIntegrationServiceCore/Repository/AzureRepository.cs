using AppStoreIntegrationServiceCore.Model;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.RetryPolicies;
using Newtonsoft.Json;
using System.Text;
using static AppStoreIntegrationServiceCore.Enums;
using System.Text.RegularExpressions;
using AppStoreIntegrationServiceCore.Repository.Interface;

namespace AppStoreIntegrationServiceCore.Repository
{
    public class AzureRepository<T> : IAzureRepository<T> where T : PluginDetails<PluginVersion<string>, string>
    {
        private readonly IConfigurationSettings _configurationSettings;
        private readonly BlobRequestOptions _blobRequestOptions;
        private CloudBlockBlob _pluginsListBlockBlobOptimized;
        private CloudBlockBlob _pluginsBackupBlockBlobOptimized;
        private CloudBlockBlob _nameMappingsBlockBlob;
        private CloudBlockBlob _settingsBlockBlob;
        private CloudBlobContainer _cloudBlobContainer;

        public AzureRepository(IConfigurationSettings configurationSettings)
        {
            _configurationSettings = configurationSettings;
            if (_configurationSettings.DeployMode != DeployMode.AzureBlob)
            {
                return;
            }

            _blobRequestOptions = new BlobRequestOptions
            {
                MaximumExecutionTime = TimeSpan.FromSeconds(120),
                RetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(3), 3),
                DisableContentMD5Validation = true,
                StoreBlobContentMD5 = false
            };

            var cloudStorageAccount = GetCloudStorageAccount();
            if (cloudStorageAccount != null)
            {
                CreateContainer(cloudStorageAccount);
            }

            SetCloudBlockBlobs();
            InitializeBlockBlobs();
        }

        public async Task<List<T>> GetPluginsFromContainer()
        {
            return (await ReadFromContainer())?.Value;
        }

        public async Task<PluginResponse<T>> ReadFromContainer()
        {
            string containerContent = await _pluginsListBlockBlobOptimized.DownloadTextAsync(Encoding.UTF8, null, _blobRequestOptions, null);
            return JsonConvert.DeserializeObject<PluginResponse<T>>(containerContent) ?? new PluginResponse<T>();
        }

        public async Task<List<ProductDetails>> GetProductsFromContainer()
        {
            return (await ReadFromContainer())?.Products;
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
            if (!string.IsNullOrEmpty(_configurationSettings.PluginsFileName))
            {
                _pluginsListBlockBlobOptimized = GetBlockBlobReference(_configurationSettings.PluginsFileName);

                var backupFileName = $"{Path.GetFileNameWithoutExtension(_configurationSettings.PluginsFileName)}_backupFile.json";
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

        private void CreateContainer(CloudStorageAccount cloudStorageAccount)
        {
            var blobClient = cloudStorageAccount.CreateCloudBlobClient();
            var blobName = NormalizeBlobName();
            _cloudBlobContainer = blobClient.GetContainerReference(blobName);

            if (_cloudBlobContainer.CreateIfNotExists())
            {
                _cloudBlobContainer.SetPermissionsAsync(new
                    BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Blob
                });
            }
        }

        private string NormalizeBlobName()
        {
            if (string.IsNullOrEmpty(_configurationSettings.BlobName))
            {
                _configurationSettings.BlobName = "defaultblobname";
            }
            var regex = new Regex("[A-Za-z0-9]+");
            var matchCollection = regex.Matches(_configurationSettings.BlobName);
            var normalizedName = string.Concat(matchCollection.Select(m => m.Value));
            if (normalizedName.Length < 3)
            {
                normalizedName = $"{normalizedName}appstore";
            }

            return normalizedName.ToLower();
        }

        private static void CreateEmptyFile(CloudBlockBlob cloudBlockBlob)
        {
            if (cloudBlockBlob is null) return;
            var fileBlobExists = cloudBlockBlob.Exists();
            if (!fileBlobExists)
            {
                cloudBlockBlob.UploadText(string.Empty);
            }
        }

        private CloudBlockBlob GetBlockBlobReference(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return null;
            }

            var cloudBlob = _cloudBlobContainer.GetBlockBlobReference(fileName);
            cloudBlob.Properties.ContentType = Path.GetExtension(fileName);
            return cloudBlob;
        }

        private CloudStorageAccount GetCloudStorageAccount()
        {
            if (string.IsNullOrEmpty(_configurationSettings?.StorageAccountName) ||
                string.IsNullOrEmpty(_configurationSettings?.StorageAccountKey))
            {
                return null;
            }

            var storageCredentils = new StorageCredentials(_configurationSettings?.StorageAccountName, _configurationSettings?.StorageAccountKey);
            var storageAccount = new CloudStorageAccount(storageCredentils, true);
            return storageAccount;
        }

        public async Task<List<ParentProduct>> GetParentProductsFromContainer()
        {
            return (await ReadFromContainer())?.ParentProducts;
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
            return (await ReadFromContainer())?.APIVersion ?? "1.0.0";
        }

        public async Task<List<CategoryDetails>> GetCategoriesFromContainer()
        {
            return (await ReadFromContainer())?.Categories;
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

        public async Task UpdateAPIVersion(string version)
        {
            var response = await ReadFromContainer();
            string text = JsonConvert.SerializeObject(new PluginResponse<T>
            {
                APIVersion = version,
                Value = response.Value,
                Products = response.Products,
                ParentProducts = response.ParentProducts,
                Categories = response.Categories
            });
            await _pluginsListBlockBlobOptimized.UploadTextAsync(text);
        }
    }
}

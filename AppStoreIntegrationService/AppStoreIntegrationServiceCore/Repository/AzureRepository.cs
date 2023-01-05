using AppStoreIntegrationServiceCore.Model;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.RetryPolicies;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;
using AppStoreIntegrationServiceCore.Repository.Interface;

namespace AppStoreIntegrationServiceCore.Repository
{
    public class AzureRepository : IResponseManager, IPluginManager, IProductsManager, IVersionManager, INamesManager, ICategoriesManager, ISettingsManager, ICommentsManager
    {
        private readonly IConfigurationSettings _configurationSettings;
        private readonly BlobRequestOptions _blobRequestOptions;
        private CloudBlockBlob _pluginsListBlockBlobOptimized;
        private CloudBlockBlob _pluginsBackupBlockBlobOptimized;
        private CloudBlockBlob _nameMappingsBlockBlob;
        private CloudBlockBlob _settingsBlockBlob;
        private CloudBlockBlob _commentsBlockBlob;
        private CloudBlobContainer _cloudBlobContainer;

        public AzureRepository(IConfigurationSettings configurationSettings)
        {
            _configurationSettings = configurationSettings;
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

        public async Task<PluginResponse<PluginDetails<PluginVersion<string>, string>>> GetResponse()
        {
            string containerContent = await _pluginsListBlockBlobOptimized.DownloadTextAsync(Encoding.UTF8, null, _blobRequestOptions, null);
            return JsonConvert.DeserializeObject<PluginResponse<PluginDetails<PluginVersion<string>, string>>>(containerContent) ?? new PluginResponse<PluginDetails<PluginVersion<string>, string>>();
        }

        public async Task<List<NameMapping>> ReadNames()
        {
            var containterContent = await _nameMappingsBlockBlob.DownloadTextAsync(Encoding.UTF8, null, _blobRequestOptions, null);
            return JsonConvert.DeserializeObject<List<NameMapping>>(containterContent) ?? new List<NameMapping>();
        }

        public async Task SaveNames(List<NameMapping> mappings)
        {
            var text = JsonConvert.SerializeObject(mappings);
            await _nameMappingsBlockBlob.UploadTextAsync(text);
        }

        public async Task<SiteSettings> ReadSettings()
        {
            var containterContent = await _settingsBlockBlob.DownloadTextAsync(Encoding.UTF8, null, _blobRequestOptions, null);
            return JsonConvert.DeserializeObject<SiteSettings>(containterContent) ?? new SiteSettings();
        }

        public async Task SaveSettings(SiteSettings settings)
        {
            var text = JsonConvert.SerializeObject(settings);
            await _settingsBlockBlob.UploadTextAsync(text);
        }

        public async Task<string> GetVersion()
        {
            return (await GetResponse())?.APIVersion;
        }

        public async Task<IEnumerable<CategoryDetails>> ReadCategories()
        {
            return (await GetResponse())?.Categories;
        }

        public async Task<IEnumerable<PluginDetails<PluginVersion<string>, string>>> ReadPlugins()
        {
            return (await GetResponse())?.Value;
        }

        public async Task<IEnumerable<ProductDetails>> ReadProducts()
        {
            return (await GetResponse())?.Products;
        }

        public async Task<IEnumerable<ParentProduct>> ReadParents()
        {
            return (await GetResponse())?.ParentProducts;
        }

        public async Task SavePlugins(IEnumerable<PluginDetails<PluginVersion<string>, string>> plugins)
        {
            var response = await GetResponse();
            response.Value = plugins;
            await _pluginsListBlockBlobOptimized.UploadTextAsync(JsonConvert.SerializeObject(response));
        }

        public async Task BackupPlugins(IEnumerable<PluginDetails<PluginVersion<string>, string>> plugins)
        {
            var response = await GetResponse();
            response.Value = plugins;
            await _pluginsBackupBlockBlobOptimized.UploadTextAsync(JsonConvert.SerializeObject(response));
        }

        public async Task SaveProducts(IEnumerable<ProductDetails> products)
        {
            var response = await GetResponse();
            response.Products = products;
            await _pluginsListBlockBlobOptimized.UploadTextAsync(JsonConvert.SerializeObject(response));
        }

        public async Task SaveProducts(IEnumerable<ParentProduct> products)
        {
            var response = await GetResponse();
            response.ParentProducts = products;
            await _pluginsListBlockBlobOptimized.UploadTextAsync(JsonConvert.SerializeObject(response));
        }

        public async Task SaveCategories(IEnumerable<CategoryDetails> categories)
        {
            var response = await GetResponse();
            response.Categories = categories;
            await _pluginsListBlockBlobOptimized.UploadTextAsync(JsonConvert.SerializeObject(response));
        }

        public async Task SaveVersion(string version)
        {
            var response = await GetResponse();
            response.APIVersion = version;
            await _pluginsListBlockBlobOptimized.UploadTextAsync(JsonConvert.SerializeObject(response));
        }

        private CloudStorageAccount GetCloudStorageAccount()
        {
            if (string.IsNullOrEmpty(_configurationSettings?.StorageAccountName) || string.IsNullOrEmpty(_configurationSettings?.StorageAccountKey))
            {
                return null;
            }

            var storageCredentils = new StorageCredentials(_configurationSettings?.StorageAccountName, _configurationSettings?.StorageAccountKey);
            return new CloudStorageAccount(storageCredentils, true);
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
            if (cloudBlockBlob is null)
            {
                return;
            }

            if (!cloudBlockBlob.Exists())
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

        private void InitializeBlockBlobs()
        {
            CreateEmptyFile(_pluginsListBlockBlobOptimized);
            CreateEmptyFile(_pluginsBackupBlockBlobOptimized);
            CreateEmptyFile(_nameMappingsBlockBlob);
            CreateEmptyFile(_settingsBlockBlob);
            CreateEmptyFile(_commentsBlockBlob);
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

            if (!string.IsNullOrEmpty(_configurationSettings.CommentsFileName))
            {
                _commentsBlockBlob = GetBlockBlobReference(_configurationSettings.CommentsFileName);
            }
        }

        private void CreateContainer(CloudStorageAccount cloudStorageAccount)
        {
            var blobName = NormalizeBlobName();
            _cloudBlobContainer = cloudStorageAccount.CreateCloudBlobClient().GetContainerReference(blobName);

            if (_cloudBlobContainer.CreateIfNotExists())
            {
                _cloudBlobContainer.SetPermissionsAsync(new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Blob
                });
            }
        }

        public async Task<IDictionary<string, IDictionary<string, IEnumerable<Comment>>>> ReadComments()
        {
            var containterContent = await _commentsBlockBlob.DownloadTextAsync(Encoding.UTF8, null, _blobRequestOptions, null);
            return JsonConvert.DeserializeObject<IDictionary<string, IDictionary<string, IEnumerable<Comment>>>>(containterContent) ?? new Dictionary<string, IDictionary<string, IEnumerable<Comment>>>();
        }

        public async Task UpdateComments(IDictionary<string, IDictionary<string, IEnumerable<Comment>>> comments)
        {
            var text = JsonConvert.SerializeObject(comments);
            await _commentsBlockBlob.UploadTextAsync(text);
        }
    }
}

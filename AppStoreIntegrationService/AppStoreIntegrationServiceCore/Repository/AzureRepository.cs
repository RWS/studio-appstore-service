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
    public class AzureRepository : IResponseManager, IPluginManager, IProductsManager, IVersionManager, INamesManager, ICategoriesManager, ISettingsManager, ICommentsManager, ILogsManager
    {
        private readonly IConfigurationSettings _configurationSettings;
        private readonly BlobRequestOptions _blobRequestOptions;
        private CloudBlockBlob _pluginsBlockBlob;
        private CloudBlockBlob _pluginsBackupBlockBlob;
        private CloudBlockBlob _settingsBlockBlob;
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

        public async Task<PluginResponse<PluginDetails>> GetResponse()
        {
            string containerContent = await _pluginsBlockBlob.DownloadTextAsync(Encoding.UTF8, null, _blobRequestOptions, null);

            if (string.IsNullOrEmpty(containerContent))
            {
                return new PluginResponse<PluginDetails>();
            }

            return JsonConvert.DeserializeObject<PluginResponse<PluginDetails>>(containerContent) ?? new PluginResponse<PluginDetails>();
        }

        public async Task<IEnumerable<NameMapping>> ReadNames()
        {
            return (await GetResponse())?.Names;
        }

        public async Task<string> GetVersion()
        {
            return (await GetResponse())?.APIVersion ?? "1.0.0";
        }

        public async Task<IEnumerable<CategoryDetails>> ReadCategories()
        {
            return (await GetResponse())?.Categories;
        }

        public async Task<IEnumerable<PluginDetails>> ReadPlugins()
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

        public async Task<IDictionary<int, CommentPackage>> ReadComments()
        {
            return (await GetResponse())?.Comments;
        }
        public async Task<IDictionary<int, IEnumerable<Log>>> ReadLogs()
        {
            return (await GetResponse())?.Logs;
        }

        public async Task<IEnumerable<PluginDetails>> ReadPending()
        {
            return (await GetResponse())?.Pending;
        }

        public async Task SavePending(IEnumerable<PluginDetails> plugins)
        {
            var response = await GetResponse();
            response.Pending = plugins;
            await _pluginsBlockBlob.UploadTextAsync(JsonConvert.SerializeObject(response));
        }

        public async Task<IEnumerable<PluginDetails>> ReadDrafts()
        {
            return (await GetResponse())?.Drafts;
        }

        public async Task SaveDrafts(IEnumerable<PluginDetails> plugins)
        {
            var response = await GetResponse();
            response.Drafts = plugins;
            await _pluginsBlockBlob.UploadTextAsync(JsonConvert.SerializeObject(response));
        }

        public async Task SaveNames(IEnumerable<NameMapping> mappings)
        {
            var response = await GetResponse();
            response.Names = mappings;
            await _pluginsBlockBlob.UploadTextAsync(JsonConvert.SerializeObject(response));
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

        public async Task SavePlugins(IEnumerable<PluginDetails> plugins)
        {
            var response = await GetResponse();
            response.Value = plugins;
            await _pluginsBlockBlob.UploadTextAsync(JsonConvert.SerializeObject(response));
        }

        public async Task SaveProducts(IEnumerable<ProductDetails> products)
        {
            var response = await GetResponse();
            response.Products = products;
            await _pluginsBlockBlob.UploadTextAsync(JsonConvert.SerializeObject(response));
        }

        public async Task SaveProducts(IEnumerable<ParentProduct> products)
        {
            var response = await GetResponse();
            response.ParentProducts = products;
            await _pluginsBlockBlob.UploadTextAsync(JsonConvert.SerializeObject(response));
        }

        public async Task SaveCategories(IEnumerable<CategoryDetails> categories)
        {
            var response = await GetResponse();
            response.Categories = categories;
            await _pluginsBlockBlob.UploadTextAsync(JsonConvert.SerializeObject(response));
        }

        public async Task SaveVersion(string version)
        {
            var response = await GetResponse();
            response.APIVersion = version;
            await _pluginsBlockBlob.UploadTextAsync(JsonConvert.SerializeObject(response));
        }

        public async Task UpdateComments(IDictionary<int, CommentPackage> comments)
        {
            var response = await GetResponse();
            response.Comments = comments;
            await _pluginsBlockBlob.UploadTextAsync(JsonConvert.SerializeObject(response));
        }

        public async Task UpdateLogs(IDictionary<int, IEnumerable<Log>> logs)
        {
            var response = await GetResponse();
            response.Logs = logs;
            await _pluginsBlockBlob.UploadTextAsync(JsonConvert.SerializeObject(response));
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
            CreateEmptyFile(_pluginsBlockBlob);
            CreateEmptyFile(_pluginsBackupBlockBlob);
            CreateEmptyFile(_settingsBlockBlob);
        }

        private void SetCloudBlockBlobs()
        {
            if (!string.IsNullOrEmpty(_configurationSettings.PluginsFileName))
            {
                _pluginsBlockBlob = GetBlockBlobReference(_configurationSettings.PluginsFileName);

                var backupFileName = $"{Path.GetFileNameWithoutExtension(_configurationSettings.PluginsFileName)}_backupFile.json";
                _pluginsBackupBlockBlob = GetBlockBlobReference(backupFileName);
            }

            if (!string.IsNullOrEmpty(_configurationSettings.SettingsFileName))
            {
                _settingsBlockBlob = GetBlockBlobReference(_configurationSettings.SettingsFileName);
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
    }
}

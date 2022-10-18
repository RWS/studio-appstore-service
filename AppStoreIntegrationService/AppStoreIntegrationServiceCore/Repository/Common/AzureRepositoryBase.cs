using AppStoreIntegrationServiceCore.Repository.Common.Interface;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.RetryPolicies;
using Microsoft.Azure.Storage;
using Newtonsoft.Json;
using static AppStoreIntegrationServiceCore.Enums;
using System.Text.RegularExpressions;
using System.Text;
using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.Common
{
    public class AzureRepositoryBase<T>
    {
        private CloudBlobContainer _cloudBlobContainer;
        private CloudBlockBlob _nameMappingsBlockBlob;
        private CloudBlockBlob _settingsBlockBlob;
        protected readonly BlobRequestOptions _blobRequestOptions;
        protected readonly IConfigurationSettings _configurationSettings;

        public AzureRepositoryBase(IConfigurationSettings configurationSettings)
        {
            _configurationSettings = configurationSettings;
            if (_configurationSettings.DeployMode != DeployMode.AzureBlob ||
                string.IsNullOrEmpty(_configurationSettings.PluginsFileNameV1))
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
        }

        public async Task<List<NameMapping>> GetNameMappingsFromContainer()
        {
            var containterContent = await _nameMappingsBlockBlob.DownloadTextAsync(Encoding.UTF8, null, _blobRequestOptions, null);
            var nameMappings = JsonConvert.DeserializeObject<List<NameMapping>>(containterContent);
            return nameMappings ?? new List<NameMapping>();
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

            SetCloudBlockBlobs();
            InitializeBlockBlobs();
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

        protected virtual void InitializeBlockBlobs()
        {
            CreateEmptyFile(_nameMappingsBlockBlob);
            CreateEmptyFile(_settingsBlockBlob);
        }

        protected static void CreateEmptyFile(CloudBlockBlob cloudBlockBlob)
        {
            if (cloudBlockBlob is null) return;
            var fileBlobExists = cloudBlockBlob.Exists();
            if (!fileBlobExists)
            {
                cloudBlockBlob.UploadText(string.Empty);
            }
        }

        protected virtual void SetCloudBlockBlobs()
        {
            if (!string.IsNullOrEmpty(_configurationSettings.MappingFileName))
            {
                _nameMappingsBlockBlob = GetBlockBlobReference(_configurationSettings.MappingFileName);
            }

            if (!string.IsNullOrEmpty(_configurationSettings.SettingsFileName))
            {
                _settingsBlockBlob = GetBlockBlobReference(_configurationSettings.SettingsFileName);
            }

        }

        protected CloudBlockBlob GetBlockBlobReference(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return null;
            }

            var cloudBlob = _cloudBlobContainer.GetBlockBlobReference(fileName);
            cloudBlob.Properties.ContentType = Path.GetExtension(fileName);
            return cloudBlob;
        }

        public async Task UpdateNameMappingsFileBlob(string fileContent)
        {
            await _nameMappingsBlockBlob.UploadTextAsync(fileContent);
        }

        public async Task<SiteSettings> GetSettingsFromContainer()
        {
            var containterContent = await _settingsBlockBlob.DownloadTextAsync(Encoding.UTF8, null, _blobRequestOptions, null);
            var settings = JsonConvert.DeserializeObject<SiteSettings>(containterContent);
            return settings ?? new SiteSettings();
        }

        public async Task UpdateSettingsFileBlob(string fileContent)
        {
            await _settingsBlockBlob.UploadTextAsync(fileContent);
        }
    }
}

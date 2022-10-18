using AppStoreIntegrationServiceCore.Repository.Common.Interface;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.RetryPolicies;
using Microsoft.Azure.Storage;
using Newtonsoft.Json;
using static AppStoreIntegrationServiceCore.Enums;
using System.Text.RegularExpressions;

namespace AppStoreIntegrationServiceCore.Repository.Common
{
    public class AzureRepositoryBase<T>
    {
        private CloudBlobContainer _cloudBlobContainer;
        protected readonly BlobRequestOptions _blobRequestOptions;
        protected readonly IConfigurationSettings _configurationSettings;

        public AzureRepositoryBase(IConfigurationSettings configurationSettings)
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

        protected static void CreateEmptyFile(CloudBlockBlob cloudBlockBlob)
        {
            if (cloudBlockBlob is null) return;
            var fileBlobExists = cloudBlockBlob.Exists();
            if (!fileBlobExists)
            {
                cloudBlockBlob.UploadText(string.Empty);
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
    }
}

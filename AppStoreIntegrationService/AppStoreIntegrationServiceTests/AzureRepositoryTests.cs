using Azure;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using Xunit;

namespace AppStoreIntegrationServiceTests
{
    public class AzureRepositoryTests
    {
        private readonly BlobServiceClient _blobServiceClient;

        public AzureRepositoryTests() 
        { 
            _blobServiceClient = new BlobServiceClient(new Uri("https://studioappstoreapi.blob.core.windows.net"), new DefaultAzureCredential());
        }

        [Fact]
        public void AzureRepositoryTest_DownloadContentFromBlob_ShouldReturnBlobContent()
        {
            var container = _blobServiceClient.GetBlobContainerClient("qapluginblob");
            var blob = container.GetBlobClient("plugins.json");
            var content = blob.DownloadContent().Value.Content.ToString();

            Assert.NotNull(content);
        }

        [Fact]
        public void AzureRepositoryTest_CreateContainer_ShouldCreateAzureContainer()
        {
            var container = _blobServiceClient.GetBlobContainerClient("testblob");
            var blob = container.GetBlobClient("test.json");

            if (!blob.Exists())
            {
                container.UploadBlob("test.json", new MemoryStream());
            }

            Assert.True(blob.Exists());
        }

        [Fact]
        public void AzureRepositoryTest_UploadContent_ShouldUploadContentToContainer()
        {
            var container = _blobServiceClient.GetBlobContainerClient("testblob");
            var blob = container.GetBlobClient("test.json");

            if (!blob.Exists())
            {
                container.UploadBlob("test.json", new MemoryStream());
            }

            blob.UploadAsync("simple text", new BlobUploadOptions());
        }

        [Fact]
        public async void AzureRepositoryTest_OverrideBlobContent_BlobContentShouldBeOverriden()
        {
            var container = _blobServiceClient.GetBlobContainerClient("testblob");
            var blob = container.GetBlobClient("test.json");

            if (!blob.Exists())
            {
                container.UploadBlob("test.json", new MemoryStream(Encoding.UTF8.GetBytes("simple text")));
            }

            await blob.UploadAsync(new MemoryStream(Encoding.UTF8.GetBytes("simple text overriden")), new BlobUploadOptions());
            var content = await blob.DownloadContentAsync();
            Assert.NotNull(content.Value.Content.ToString());
        }
    }
}

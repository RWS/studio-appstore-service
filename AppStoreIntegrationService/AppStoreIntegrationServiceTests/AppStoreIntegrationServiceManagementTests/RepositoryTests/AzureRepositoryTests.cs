using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using AppStoreIntegrationServiceManagement.Model.Settings;
using AppStoreIntegrationServiceManagement.Repository.Interface;
using AppStoreIntegrationServiceTests.AppStoreIntegrationServiceManagementTests.Mock;
using Xunit;

namespace AppStoreIntegrationServiceTests.AppStoreIntegrationServiceManagementTests.RepositoryTests
{
    public class AzureRepositoryTests
    {
        [Fact]
        public async void AzureRepositoryTest_GetPluginResponseWhenBlobIsEmpty_ShouldReturnEmptyResponse()
        {
            IResponseManager repository = new AzureRepositoryMock(data: null);
            var actual = await repository.GetResponse();
            var expected = new PluginResponse<PluginDetails>();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async void AzureRepositoryTest_GetPluginResponseWhenBlobIsNotEmpty_ShouldReturnExpectedData()
        {
            IResponseManager repository = new AzureRepositoryMock(new PluginResponse<PluginDetails>());
            var actual = await repository.GetResponse();
            var expected = new PluginResponse<PluginDetails>();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async void AzureRepositoryTest_GetSettingsWhenBlobIsEmpty_ShouldReturnEmptySettings()
        {
            ISettingsManager repository = new AzureRepositoryMock(settings: null);
            var actual = await repository.ReadSettings();
            var expected = new SiteSettings();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async void AzureRepositoryTest_GetSettingsWhenBlobIsNotEmpty_ShouldReturnExpectedSettings()
        {
            ISettingsManager repository = new AzureRepositoryMock(new SiteSettings { Name = "Test" });
            var actual = await repository.ReadSettings();
            var expected = new SiteSettings { Name = "Test" };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async void AzureRepositoryTest_SaveResponse_DataShouldBeSaved()
        {
            IResponseManager repository = new AzureRepositoryMock();
            var response = new PluginResponse<PluginDetails>
            {
                Categories = new List<CategoryDetails>
                {
                    new CategoryDetails{ Id = "1", Name = "Test" }
                }
            };

            await repository.SaveResponse(response);
            Assert.Equal(response, await repository.GetResponse());
        }

        [Fact]
        public async void AzureRepositoryTest_SaveSettings_DataShouldBeSaved()
        {
            ISettingsManager repository = new AzureRepositoryMock();
            var settings = new SiteSettings { Name = "Test" };
            await repository.SaveSettings(settings);
            Assert.Equal(settings, await repository.ReadSettings());
        }
    }
}

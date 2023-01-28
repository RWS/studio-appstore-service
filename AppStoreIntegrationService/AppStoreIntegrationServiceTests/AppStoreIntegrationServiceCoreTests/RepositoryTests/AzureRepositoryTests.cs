using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using AppStoreIntegrationServiceTests.AppStoreIntegrationServiceCoreTests.Mock;
using Newtonsoft.Json;
using Xunit;

namespace AppStoreIntegrationServiceTests.AppStoreIntegrationServiceCoreTests.RepositoryTests
{
    public class AzureRepositoryTests
    {
        [Fact]
        public async void AzureRepositoryTest_GetPluginResponseWhenBlobIsEmpty_ShouldReturnEmptyResponse()
        {
            IResponseManager repository = new AzureRepositoryMock(null);
            var actual = await repository.GetResponse();
            var expected = new PluginResponse<PluginDetails>();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async void AzureRepositoryTest_GetAPIVersionWhenBlobIsEmpty_ShouldReturnNull()
        {
            IVersionManager repository = new AzureRepositoryMock(null);
            Assert.Null(await repository.GetVersion());
        }

        [Fact]
        public async void AzureRepositoryTest_GetAPIVersionWhenBlobHasData_ShouldReturnTheAPIVersion()
        {
            IVersionManager repository = new AzureRepositoryMock(JsonConvert.SerializeObject(new PluginResponse<PluginDetails> { APIVersion = "1.0.0" }));
            Assert.Equal("1.0.0", await repository.GetVersion());
        }

        [Fact]
        public async void AzureRepositoryTest_GetLogsWhenBlobIsNotEmpty_ShouldReturnNotEmptyList()
        {
            var data = JsonConvert.SerializeObject(new PluginResponse<PluginDetails>
            {
                Logs = new Dictionary<int, IEnumerable<Log>>
                {
                    [0] = new List<Log>
                    {
                        new Log { Author = "Test Author", Date = DateTime.Now, Description = "Test description" }
                    }
                }
            });

            ILogsManager repository = new AzureRepositoryMock(data);
            Assert.NotEmpty(await repository.ReadLogs());
        }

        [Fact]
        public async void AzureRepositoryTest_GetLogsWhenBlobIsEmpty_ShouldReturnEmptyList()
        {
            ILogsManager repository = new AzureRepositoryMock(null);
            Assert.Empty(await repository.ReadLogs());
        }
    }
}

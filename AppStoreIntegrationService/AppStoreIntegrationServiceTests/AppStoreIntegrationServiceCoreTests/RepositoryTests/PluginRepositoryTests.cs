using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository;
using AppStoreIntegrationServiceCore.Repository.Interface;
using AppStoreIntegrationServiceTests.AppStoreIntegrationServiceCoreTests.Mock;
using Xunit;

namespace AppStoreIntegrationServiceTests.AppStoreIntegrationServiceCoreTests.RepositoryTests
{
    public class PluginRepositoryTests
    {
        [Fact]
        public async void PluginRepositoryTest_GetAllPlugins_ShouldReturnAllPluginsFromStorage()
        {
            IResponseManager repository = new AzureRepositoryMock(new PluginResponse<PluginDetails>
            {
                Value = new List<PluginDetails>
                {
                    new PluginDetails { Id = 0 },
                    new PluginDetails { Id = 1 }
                }
            });

            var pluginRepository = new PluginRepository(repository);
            var plugins = await pluginRepository.GetAll(null);
            Assert.Equal(new List<PluginDetails>
            {
                new PluginDetails { Id = 0 },
                new PluginDetails { Id = 1 }
            }, plugins);
            Assert.Equal(2, plugins.Count());
        }
    }
}

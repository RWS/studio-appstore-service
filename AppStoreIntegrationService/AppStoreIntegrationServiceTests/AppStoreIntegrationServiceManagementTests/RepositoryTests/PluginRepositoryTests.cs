using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using AppStoreIntegrationServiceManagement.Repository;
using AppStoreIntegrationServiceTests.AppStoreIntegrationServiceManagementTests.Mock;
using Xunit;

namespace AppStoreIntegrationServiceTests.AppStoreIntegrationServiceManagementTests.RepositoryTests
{
    public class PluginRepositoryTests
    {
        [Fact]
        public async void PluginRepositoryTest_GetAllDistinctPluginsInDescendingOrder_ShouldReturnAllCorresponsingPluginsFromStorage()
        {
            IResponseManager repository = new AzureRepositoryMock(InitPluginResponse());
            var pluginRepository = new PluginRepository(repository);
            var plugins = await pluginRepository.GetAll(null);

            Assert.Equal(new List<PluginDetails>
            {
                new PluginDetails { Id = 5, Name = "5 Test", HasAdminConsent = true, Status = Status.Draft, Developer = new DeveloperDetails { DeveloperName = "Test username 4" } },
                new PluginDetails { Id = 4, Name = "4 Test", HasAdminConsent = false, Status = Status.Draft, Developer = new DeveloperDetails { DeveloperName = "Test username 3" } },
                new PluginDetails { Id = 3, Name = "3 Test", HasAdminConsent = true, Status = Status.InReview, Developer = new DeveloperDetails { DeveloperName = "Test username 2" } },
                new PluginDetails { Id = 2, Name = "2 Test", HasAdminConsent = true, Status = Status.Active, Developer = new DeveloperDetails { DeveloperName = "Test username 1" } },
                new PluginDetails { Id = 1, Name = "1 Test", Status = Status.Active },
                new PluginDetails { Id = 0, Name = "0 Test", Status = Status.Inactive }
            }, plugins);
            Assert.Equal(6, plugins.Count());
        }

        [Fact]
        public async void PluginRepositoryTest_GetAllDistinctPluginsInDescendingOrder_ShouldReturnAllCorresponsingPluginsOrderedFromStorage()
        {
            IResponseManager repository = new AzureRepositoryMock(InitPluginResponse());
            var pluginRepository = new PluginRepository(repository);
            var plugins = await pluginRepository.GetAll("asc");

            Assert.Equal(new List<PluginDetails>
            {
                new PluginDetails { Id = 0, Name = "0 Test", Status = Status.Inactive },
                new PluginDetails { Id = 1, Name = "1 Test", Status = Status.Active },
                new PluginDetails { Id = 2, Name = "2 Test", HasAdminConsent = true, Status = Status.Active, Developer = new DeveloperDetails { DeveloperName = "Test username 1" } },
                new PluginDetails { Id = 3, Name = "3 Test", HasAdminConsent = true, Status = Status.InReview, Developer = new DeveloperDetails { DeveloperName = "Test username 2" } },
                new PluginDetails { Id = 4, Name = "4 Test", HasAdminConsent = false, Status = Status.Draft, Developer = new DeveloperDetails { DeveloperName = "Test username 3" }  },
                new PluginDetails { Id = 5, Name = "5 Test", HasAdminConsent = true, Status = Status.Draft, Developer = new DeveloperDetails { DeveloperName = "Test username 4" } },
            }, plugins);
            Assert.Equal(6, plugins.Count());
        }

        [Fact]
        public async void PluginRepositoryTest_GetAllDrafts_ShouldReturnAllCorresponsingPluginsFromStorage()
        {
            IResponseManager repository = new AzureRepositoryMock(InitPluginResponse());
            var pluginRepository = new PluginRepository(repository);
            var plugins = await pluginRepository.GetAll("asc", status: Status.Draft);

            Assert.Equal(new List<PluginDetails>
            {
                new PluginDetails { Id = 2, Name = "2 Test", HasAdminConsent = false, Status = Status.Draft, Developer = new DeveloperDetails { DeveloperName = "Test username 1" } },
                new PluginDetails { Id = 4, Name = "4 Test", HasAdminConsent = false, Status = Status.Draft, Developer = new DeveloperDetails { DeveloperName = "Test username 3" } },
                new PluginDetails { Id = 5, Name = "5 Test", HasAdminConsent = true, Status = Status.Draft, Developer = new DeveloperDetails { DeveloperName = "Test username 4" } },
            }, plugins);
            Assert.Equal(3, plugins.Count());
        }

        [Fact]
        public async void PluginRepositoryTest_GetAllInReview_ShouldReturnAllCorresponsingPluginsFromStorage()
        {
            IResponseManager repository = new AzureRepositoryMock(InitPluginResponse());
            var pluginRepository = new PluginRepository(repository);
            var plugins = await pluginRepository.GetAll("asc", status: Status.InReview);

            Assert.Equal(new List<PluginDetails>
            {
                new PluginDetails { Id = 2, Name = "2 Test", HasAdminConsent = true, Status = Status.InReview, Developer = new DeveloperDetails { DeveloperName = "Test username 1" } },
                new PluginDetails { Id = 3, Name = "3 Test", HasAdminConsent = true, Status = Status.InReview, Developer = new DeveloperDetails { DeveloperName = "Test username 2" } }
            }, plugins);
            Assert.Equal(2, plugins.Count());
        }

        [Theory]
        [InlineData(Status.Active)]
        [InlineData(Status.Inactive)]
        public async void PluginRepositoryTest_GetAllProductionPlugins_ShouldReturnAllCorresponsingPluginsFromStorage(Status status)
        {
            IResponseManager repository = new AzureRepositoryMock(InitPluginResponse());
            var pluginRepository = new PluginRepository(repository);
            var plugins = await pluginRepository.GetAll("asc", status: status);

            Assert.Equal(new List<PluginDetails>
            {
                new PluginDetails { Id = 0, Name = "0 Test", Status = Status.Inactive },
                new PluginDetails { Id = 1, Name = "1 Test", Status = Status.Active },
                new PluginDetails { Id = 2, Name = "2 Test", HasAdminConsent = true, Status = Status.Active, Developer = new DeveloperDetails { DeveloperName = "Test username 1" }}
            }, plugins);
            Assert.Equal(3, plugins.Count());
        }

        [Fact]
        public async void PluginRepositoryTest_GetAllDraftsForACertainDeveloper_ShouldReturnAllCorresponsingPluginsFromStorage()
        {
            IResponseManager repository = new AzureRepositoryMock(InitPluginResponse());
            var pluginRepository = new PluginRepository(repository);
            var plugins = await pluginRepository.GetAll("asc", "Test username 3", "Developer", Status.Draft);

            Assert.Equal(new List<PluginDetails>
            {
                new PluginDetails { Id = 4, Name = "4 Test", HasAdminConsent = false, Status = Status.Draft, Developer = new DeveloperDetails { DeveloperName = "Test username 3" } }
            }, plugins);
            Assert.Single(plugins);
        }

        [Fact]
        public async void PluginRepositoryTest_GetAllInReviewPluginsForACertainDeveloper_ShouldReturnAllCorresponsingPluginsFromStorage()
        {
            IResponseManager repository = new AzureRepositoryMock(InitPluginResponse());
            var pluginRepository = new PluginRepository(repository);
            var plugins = await pluginRepository.GetAll("asc", "Test username 2", "Developer", Status.InReview);

            Assert.Equal(new List<PluginDetails>
            {
                new PluginDetails { Id = 3, Name = "3 Test", HasAdminConsent = true, Status = Status.InReview, Developer = new DeveloperDetails { DeveloperName = "Test username 2" } }
            }, plugins);
            Assert.Single(plugins);
        }

        [Theory]
        [InlineData(Status.Active)]
        [InlineData(Status.Inactive)]
        public async void PluginRepositoryTest_GetAllProductionPluginsForADeveloper_ShouldReturnAllCorresponsingPluginsFromStorage(Status status)
        {
            IResponseManager repository = new AzureRepositoryMock(InitPluginResponse());
            var pluginRepository = new PluginRepository(repository);
            var plugins = await pluginRepository.GetAll("asc", "Test username 1", "Developer", status);

            Assert.Equal(new List<PluginDetails>
            {
                new PluginDetails { Id = 2, Name = "2 Test", HasAdminConsent = true, Status = Status.Active, Developer = new DeveloperDetails { DeveloperName = "Test username 1" }}
            }, plugins);
            Assert.Single(plugins);
        }

        [Fact]
        public async void PluginRepositoryTest_GetAllPriorityPluginsForADeveloper_ShouldReturnAllCorresponsingPluginsFromStorage()
        {
            IResponseManager repository = new AzureRepositoryMock(InitPluginResponse());
            var pluginRepository = new PluginRepository(repository);
            var plugins = await pluginRepository.GetAll("asc", "Test username 1", "Developer");

            Assert.Equal(new List<PluginDetails>
            {
                new PluginDetails { Id = 2, Name = "2 Test", HasAdminConsent = true, Status = Status.Active, Developer = new DeveloperDetails { DeveloperName = "Test username 1" }}
            }, plugins);
            Assert.Single(plugins);
        }

        [Fact]
        public async void PluginRepositoryTest_GetAllPluginsAvailableForAdministrators_ShouldReturnAllCorresponsingPluginsFromStorage()
        {
            IResponseManager repository = new AzureRepositoryMock(InitPluginResponse());
            var pluginRepository = new PluginRepository(repository);
            var plugins = await pluginRepository.GetAll("asc", userRole: "SystemAdministrator");

            Assert.Equal(new List<PluginDetails>
            {
                new PluginDetails { Id = 0, Name = "0 Test", Status = Status.Inactive },
                new PluginDetails { Id = 1, Name = "1 Test", Status = Status.Active },
                new PluginDetails { Id = 2, Name = "2 Test", HasAdminConsent = true, Status = Status.Active, Developer = new DeveloperDetails { DeveloperName = "Test username 1" }},
                new PluginDetails { Id = 3, Name = "3 Test", HasAdminConsent = true, Status = Status.InReview, Developer = new DeveloperDetails { DeveloperName = "Test username 2" } },
                new PluginDetails { Id = 5, Name = "5 Test", HasAdminConsent = true, Status = Status.Draft, Developer = new DeveloperDetails { DeveloperName = "Test username 4" } },
            }, plugins);
            Assert.Equal(5, plugins.Count());
        }

        [Fact]
        public async void PluginRepositoryTest_GetAllDraftsAvailableForAdministrators_ShouldReturnAllCorresponsingPluginsFromStorage()
        {
            IResponseManager repository = new AzureRepositoryMock(InitPluginResponse());
            var pluginRepository = new PluginRepository(repository);

            var plugins = await pluginRepository.GetAll("asc", userRole: "SystemAdministrator", status: Status.Draft);

            Assert.Equal(new List<PluginDetails>
            {
                   new PluginDetails { Id = 5, Name = "5 Test", HasAdminConsent = true, Status = Status.Draft, Developer = new DeveloperDetails { DeveloperName = "Test username 4" } },
            }, plugins);
            Assert.Single(plugins);
        }

        [Fact]
        public async void PluginRepositoryTest_GetAllPendingPluginsAvailableForAdministrators_ShouldReturnAllCorresponsingPluginsFromStorage()
        {
            IResponseManager repository = new AzureRepositoryMock(InitPluginResponse());
            var pluginRepository = new PluginRepository(repository);
            var plugins = await pluginRepository.GetAll("asc", userRole: "SystemAdministrator", status: Status.InReview);

            Assert.Equal(new List<PluginDetails>
            {
                new PluginDetails { Id = 2, Name = "2 Test", HasAdminConsent = true, Status = Status.InReview, Developer = new DeveloperDetails { DeveloperName = "Test username 1" } },
                new PluginDetails { Id = 3, Name = "3 Test", HasAdminConsent = true, Status = Status.InReview, Developer = new DeveloperDetails { DeveloperName = "Test username 2" } }
            }, plugins);
            Assert.Equal(2, plugins.Count());
        }

        [Theory]
        [InlineData(Status.Active)]
        [InlineData(Status.Inactive)]
        public async void PluginRepositoryTest_GetAllProductionPluginsAvailableForAdministrators_ShouldReturnAllCorresponsingPluginsFromStorage(Status status)
        {
            IResponseManager repository = new AzureRepositoryMock(InitPluginResponse());
            var pluginRepository = new PluginRepository(repository);
            var plugins = await pluginRepository.GetAll("asc", userRole: "SystemAdministrator", status: status);

            Assert.Equal(new List<PluginDetails>
            {
                new PluginDetails { Id = 0, Name = "0 Test", Status = Status.Inactive },
                new PluginDetails { Id = 1, Name = "1 Test", Status = Status.Active },
                new PluginDetails { Id = 2, Name = "2 Test", HasAdminConsent = true, Status = Status.Active, Developer = new DeveloperDetails { DeveloperName = "Test username 1" }}
            }, plugins);
            Assert.Equal(3, plugins.Count());
        }

        [Fact]

        private static PluginResponse<PluginDetails> InitPluginResponse()
        {
            return new PluginResponse<PluginDetails>
            {
                Value = new List<PluginDetails>
                {
                    new PluginDetails { Id = 0, Name = "0 Test", Status = Status.Inactive },
                    new PluginDetails { Id = 1, Name = "1 Test", Status = Status.Active },
                    new PluginDetails { Id = 2, Name = "2 Test", HasAdminConsent = true, Status = Status.Active, Developer = new DeveloperDetails { DeveloperName = "Test username 1" }}
                },
                Pending = new List<PluginDetails>
                {
                    new PluginDetails { Id = 2, Name = "2 Test", HasAdminConsent = true, Status = Status.InReview, Developer = new DeveloperDetails { DeveloperName = "Test username 1" } },
                    new PluginDetails { Id = 3, Name = "3 Test", HasAdminConsent = true, Status = Status.InReview, Developer = new DeveloperDetails { DeveloperName = "Test username 2" } }
                },
                Drafts = new List<PluginDetails>
                {
                    new PluginDetails { Id = 2, Name = "2 Test", HasAdminConsent = false, Status = Status.Draft, Developer = new DeveloperDetails { DeveloperName = "Test username 1" } },
                    new PluginDetails { Id = 4, Name = "4 Test", HasAdminConsent = false, Status = Status.Draft, Developer = new DeveloperDetails { DeveloperName = "Test username 3" } },
                    new PluginDetails { Id = 5, Name = "5 Test", HasAdminConsent = true, Status = Status.Draft, Developer = new DeveloperDetails { DeveloperName = "Test username 4" } },
                }
            };
        }
    }
}

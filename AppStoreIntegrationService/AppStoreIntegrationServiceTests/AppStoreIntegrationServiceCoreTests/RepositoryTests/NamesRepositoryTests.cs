using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository;
using AppStoreIntegrationServiceTests.AppStoreIntegrationServiceCoreTests.Mock;
using Xunit;

namespace AppStoreIntegrationServiceTests.AppStoreIntegrationServiceCoreTests.RepositoryTests
{
    public class NamesRepositoryTests
    {
        [Fact]
        public async void NamesRepositoryTest_GetAllNamesFromNullOrEmptyStorage_ShouldReturnEmptyList()
        {
            var azurerepository = new AzureRepositoryMock(data: null);
            var namesRepository = new NamesRepository(azurerepository);

            Assert.Empty(await namesRepository.GetAllNames());
        }

        [Fact]
        public async void NamesRepositoryTest_GetAllNamesFromStorage_ShouldReturnAllNames()
        {
            var azurerepository = new AzureRepositoryMock(new PluginResponse<PluginDetails>
            {
                Names = new List<NameMapping>
                {
                    new NameMapping { NewName = "Test new name 1", OldName = "Test old name 1" },
                    new NameMapping { NewName = "Test new name 2", OldName = "Test old name 2" },
                    new NameMapping { NewName = "Test new name 3", OldName = "Test old name 3" }
                }
            });

            var namesRepository = new NamesRepository(azurerepository);
            Assert.Equal(new List<NameMapping>
            {
                new NameMapping { NewName = "Test new name 1", OldName = "Test old name 1" },
                new NameMapping { NewName = "Test new name 2", OldName = "Test old name 2" },
                new NameMapping { NewName = "Test new name 3", OldName = "Test old name 3" }
            }, await namesRepository.GetAllNames());
        }

        [Fact]
        public async void NamesRepositoryTest_GetAllNamesFromStorageByPluginName_ShouldReturnAllCorrespondingNames()
        {
            var azurerepository = new AzureRepositoryMock(new PluginResponse<PluginDetails>
            {
                Names = new List<NameMapping>
                {
                    new NameMapping { NewName = "Test new name 1", OldName = "Test old name 1" },
                    new NameMapping { NewName = "Test new name 2", OldName = "Test old name 2" },
                    new NameMapping { NewName = "Test new name 3", OldName = "Test old name 3" }
                }
            });

            var namesRepository = new NamesRepository(azurerepository);
            Assert.Equal(new List<NameMapping>
            {
                new NameMapping { NewName = "Test new name 1", OldName = "Test old name 1" },
            }, await namesRepository.GetAllNames(new List<string> { "Test old name 1" }));
        }

        [Fact]
        public async void NamesRepositoryTest_GetAllNamesFromStorageByPluginNameWhenListIsNull_ShouldReturnEmptyList()
        {
            var azurerepository = new AzureRepositoryMock(new PluginResponse<PluginDetails>
            {
                Names = new List<NameMapping>
                {
                    new NameMapping { NewName = "Test new name 1", OldName = "Test old name 1" },
                    new NameMapping { NewName = "Test new name 2", OldName = "Test old name 2" },
                    new NameMapping { NewName = "Test new name 3", OldName = "Test old name 3" }
                }
            });

            var namesRepository = new NamesRepository(azurerepository);
            Assert.Empty(await namesRepository.GetAllNames(null));
        }

        [Fact]
        public async void NamesRepositoryTest_UpdateInexistentNameMapping_NameMappingShouldBeAddedToStorage()
        {
            var azurerepository = new AzureRepositoryMock(new PluginResponse<PluginDetails>
            {
                Names = new List<NameMapping>
                {
                    new NameMapping { NewName = "Test new name 1", OldName = "Test old name 1" },
                    new NameMapping { NewName = "Test new name 2", OldName = "Test old name 2" },
                    new NameMapping { NewName = "Test new name 3", OldName = "Test old name 3" }
                }
            });

            var namesRepository = new NamesRepository(azurerepository);
            Assert.True(await namesRepository.TryUpdateMapping(new NameMapping { Id = "1", NewName = "Test new name 4", OldName = "Test old name 4" }));
            Assert.Equal(new List<NameMapping>
            {
                new NameMapping { NewName = "Test new name 1", OldName = "Test old name 1" },
                new NameMapping { NewName = "Test new name 2", OldName = "Test old name 2" },
                new NameMapping { NewName = "Test new name 3", OldName = "Test old name 3" },
                new NameMapping { Id = "1", NewName = "Test new name 4", OldName = "Test old name 4" }
            }, await namesRepository.GetAllNames());
        }

        [Fact]
        public async void NamesRepositoryTest_UpdateExistentNameMapping_NameMappingShouldBeAddedToStorage()
        {
            var azurerepository = new AzureRepositoryMock(new PluginResponse<PluginDetails>
            {
                Names = new List<NameMapping>
                {
                    new NameMapping { Id = "1", NewName = "Test new name 1", OldName = "Test old name 1" },
                    new NameMapping { Id = "2", NewName = "Test new name 2", OldName = "Test old name 2" },
                    new NameMapping { Id = "3", NewName = "Test new name 3", OldName = "Test old name 3" }
                }
            });

            var namesRepository = new NamesRepository(azurerepository);
            Assert.True(await namesRepository.TryUpdateMapping(new NameMapping { Id = "3", NewName = "Test new name 3 - updated", OldName = "Test old name 3 - updated" }));
            Assert.Equal(new List<NameMapping>
            {
                new NameMapping { Id = "1", NewName = "Test new name 1", OldName = "Test old name 1" },
                new NameMapping { Id = "2", NewName = "Test new name 2", OldName = "Test old name 2" },
                new NameMapping { Id = "3", NewName = "Test new name 3 - updated", OldName = "Test old name 3 - updated" },
            }, await namesRepository.GetAllNames());
        }

        [Fact]
        public async void NamesRepositoryTest_UpdateNullNameMapping_NameMappingShouldNotBeAddedToStorage()
        {
            var azurerepository = new AzureRepositoryMock(new PluginResponse<PluginDetails>
            {
                Names = new List<NameMapping>
                {
                    new NameMapping { Id = "1", NewName = "Test new name 1", OldName = "Test old name 1" },
                    new NameMapping { Id = "2", NewName = "Test new name 2", OldName = "Test old name 2" },
                    new NameMapping { Id = "3", NewName = "Test new name 3", OldName = "Test old name 3" }
                }
            });

            var namesRepository = new NamesRepository(azurerepository);
            Assert.False(await namesRepository.TryUpdateMapping(null));
            Assert.Equal(new List<NameMapping>
            {
                new NameMapping { Id = "1", NewName = "Test new name 1", OldName = "Test old name 1" },
                new NameMapping { Id = "2", NewName = "Test new name 2", OldName = "Test old name 2" },
                new NameMapping { Id = "3", NewName = "Test new name 3", OldName = "Test old name 3" }
            }, await namesRepository.GetAllNames());
        }

        [Fact]
        public async void NamesRepositoryTest_UpdateDuplicateNameMapping_NameMappingShouldNotBeAddedToStorage()
        {
            var azurerepository = new AzureRepositoryMock(new PluginResponse<PluginDetails>
            {
                Names = new List<NameMapping>
                {
                    new NameMapping { Id = "1", NewName = "Test new name 1", OldName = "Test old name 1" },
                    new NameMapping { Id = "2", NewName = "Test new name 2", OldName = "Test old name 2" },
                    new NameMapping { Id = "3", NewName = "Test new name 3", OldName = "Test old name 3" }
                }
            });

            var namesRepository = new NamesRepository(azurerepository);
            Assert.False(await namesRepository.TryUpdateMapping(new NameMapping { Id = "4", NewName = "Test new name 1", OldName = "Test old name 1" }));
            Assert.Equal(new List<NameMapping>
            {
                new NameMapping { Id = "1", NewName = "Test new name 1", OldName = "Test old name 1" },
                new NameMapping { Id = "2", NewName = "Test new name 2", OldName = "Test old name 2" },
                new NameMapping { Id = "3", NewName = "Test new name 3", OldName = "Test old name 3" }
            }, await namesRepository.GetAllNames());
        }

        [Fact]
        public async void NamesRepositoryTest_DeleteNameMappingById_RecordShouldBeRemoved()
        {
            var azurerepository = new AzureRepositoryMock(new PluginResponse<PluginDetails>
            {
                Names = new List<NameMapping>
                {
                    new NameMapping { Id = "1", NewName = "Test new name 1", OldName = "Test old name 1" },
                    new NameMapping { Id = "2", NewName = "Test new name 2", OldName = "Test old name 2" },
                    new NameMapping { Id = "3", NewName = "Test new name 3", OldName = "Test old name 3" }
                }
            });

            var namesRepository = new NamesRepository(azurerepository);
            await namesRepository.DeleteMapping("1");

            Assert.Equal(new List<NameMapping>
            {
                new NameMapping { Id = "2", NewName = "Test new name 2", OldName = "Test old name 2" },
                new NameMapping { Id = "3", NewName = "Test new name 3", OldName = "Test old name 3" }
            }, await namesRepository.GetAllNames());
        }

        [Fact]
        public async void NamesRepositoryTest_DeleteNameMappingByNullId_NamesShouldBeUnchanged()
        {
            var azurerepository = new AzureRepositoryMock(new PluginResponse<PluginDetails>
            {
                Names = new List<NameMapping>
                {
                    new NameMapping { Id = "1", NewName = "Test new name 1", OldName = "Test old name 1" },
                    new NameMapping { Id = "2", NewName = "Test new name 2", OldName = "Test old name 2" },
                    new NameMapping { Id = "3", NewName = "Test new name 3", OldName = "Test old name 3" }
                }
            });

            var namesRepository = new NamesRepository(azurerepository);
            await namesRepository.DeleteMapping(null);

            Assert.Equal(new List<NameMapping>
            {
                new NameMapping { Id = "1", NewName = "Test new name 1", OldName = "Test old name 1" },
                new NameMapping { Id = "2", NewName = "Test new name 2", OldName = "Test old name 2" },
                new NameMapping { Id = "3", NewName = "Test new name 3", OldName = "Test old name 3" }
            }, await namesRepository.GetAllNames());
        }
    }
}

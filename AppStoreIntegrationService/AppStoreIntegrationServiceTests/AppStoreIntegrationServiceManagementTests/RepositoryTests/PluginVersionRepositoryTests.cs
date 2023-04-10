using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceManagement.Repository;
using Xunit;
using AppStoreIntegrationServiceCore.Repository.Interface;
using AppStoreIntegrationServiceTests.AppStoreIntegrationServiceManagementTests.Mock;

namespace AppStoreIntegrationServiceTests.AppStoreIntegrationServiceManagementTests.RepositoryTests
{
    public class PluginVersionRepositoryTests
    {
        [Fact]
        public async void PluginVersionRepositoryTest_GetVersionByIdWhenPluginIdIsNegative_ShouldReturnNull()
        {
            IResponseManager repository = new AzureRepositoryMock(InitPluginResponse());
            var pluginRepository = new PluginRepository(repository);
            var pluginVersionRepository = new PluginVersionRepository(pluginRepository);
            var version = await pluginVersionRepository.GetPluginVersion(-1, "f32b8f18-0823-49d0-b8c6-21225cafcc81");

            Assert.Null(version);
        }

        [Fact]
        public async void PluginVersionRepositoryTest_GetVersionByIdWhenVersionIdIsNullOrEmpty_ShouldReturnNull()
        {
            IResponseManager repository = new AzureRepositoryMock(InitPluginResponse());
            var pluginRepository = new PluginRepository(repository);
            var pluginVersionRepository = new PluginVersionRepository(pluginRepository);
            var version = await pluginVersionRepository.GetPluginVersion(-1, "");

            Assert.Null(version);
        }

        [Fact]
        public async void PluginVersionRepositoryTest_GetVersionByIdWhenStatusNotSpecified_ShouldReturnTheBiggestPriorityVersion()
        {
            IResponseManager repository = new AzureRepositoryMock(InitPluginResponse());
            var pluginRepository = new PluginRepository(repository);
            var pluginVersionRepository = new PluginVersionRepository(pluginRepository);
            var version = await pluginVersionRepository.GetPluginVersion(0, "f32b8f18-0823-49d0-b8c6-21225cafcc81");

            Assert.Equal(new PluginVersion { VersionId = "f32b8f18-0823-49d0-b8c6-21225cafcc81", VersionStatus = Status.Active }, version);
        }

        [Fact]
        public async void PluginVersionRepositoryTest_GetVersionByIdWhenStatusIsActive_ShouldReturnTheCorrespondingVersion()
        {
            IResponseManager repository = new AzureRepositoryMock(InitPluginResponse());
            var pluginRepository = new PluginRepository(repository);
            var pluginVersionRepository = new PluginVersionRepository(pluginRepository);
            var version = await pluginVersionRepository.GetPluginVersion(0, "f32b8f18-0823-49d0-b8c6-21225cafcc81", Status.Active);

            Assert.Equal(new PluginVersion { VersionId = "f32b8f18-0823-49d0-b8c6-21225cafcc81", VersionStatus = Status.Active }, version);
        }

        [Fact]
        public async void PluginVersionRepositoryTest_GetVersionByIdWhenStatusIsInReview_ShouldReturnTheCorrespondingVersion()
        {
            IResponseManager repository = new AzureRepositoryMock(InitPluginResponse());
            var pluginRepository = new PluginRepository(repository);
            var pluginVersionRepository = new PluginVersionRepository(pluginRepository);
            var version = await pluginVersionRepository.GetPluginVersion(0, "f32b8f18-0823-49d0-b8c6-21225cafcc81", Status.InReview);

            Assert.Equal(new PluginVersion { VersionId = "f32b8f18-0823-49d0-b8c6-21225cafcc81", VersionStatus = Status.InReview }, version);
        }

        [Fact]
        public async void PluginVersionRepositoryTest_GetVersionByIdWhenStatusIsDraft_ShouldReturnTheCorrespondingVersion()
        {
            IResponseManager repository = new AzureRepositoryMock(InitPluginResponse());
            var pluginRepository = new PluginRepository(repository);
            var pluginVersionRepository = new PluginVersionRepository(pluginRepository);
            var version = await pluginVersionRepository.GetPluginVersion(0, "f32b8f18-0823-49d0-b8c6-21225cafcc81", Status.Draft);

            Assert.Equal(new PluginVersion { VersionId = "f32b8f18-0823-49d0-b8c6-21225cafcc81", VersionStatus = Status.Draft }, version);
        }

        [Fact]
        public async void PluginVersionRepositoryTest_GetAllVersionsWhenStatusNotSpecified_ShouldReturnAllAvailableVersionsByPriority()
        {
            IResponseManager repository = new AzureRepositoryMock(InitPluginResponse());
            var pluginRepository = new PluginRepository(repository);
            var pluginVersionRepository = new PluginVersionRepository(pluginRepository);
            var versions = await pluginVersionRepository.GetPluginVersions(0);

            Assert.Equal(new[]
            {
                new PluginVersion { VersionId = "f32b8f18-0823-49d0-b8c6-21225cafcc81", VersionStatus = Status.Active },
                new PluginVersion { VersionId = "e04e29da-6429-454c-b4fe-0d831e1e7c56", VersionStatus = Status.InReview },
                new PluginVersion { VersionId = "0f65f21c-dcb1-4648-8b05-38daf657e28d", VersionStatus = Status.Draft }

            }, versions);
            Assert.Equal(3, versions.Count());
        }

        [Fact]
        public async void PluginVersionRepositoryTest_GetAllVersionsWhenStatusIsActive_ShouldReturnAllActiveVersions()
        {
            IResponseManager repository = new AzureRepositoryMock(InitPluginResponse());
            var pluginRepository = new PluginRepository(repository);
            var pluginVersionRepository = new PluginVersionRepository(pluginRepository);
            var versions = await pluginVersionRepository.GetPluginVersions(0, Status.Active);

            Assert.Equal(new[]
            {
                new PluginVersion { VersionId = "f32b8f18-0823-49d0-b8c6-21225cafcc81", VersionStatus = Status.Active }

            }, versions);
            Assert.Single(versions);
        }

        [Fact]
        public async void PluginVersionRepositoryTest_GetAllVersionsWhenStatusIsInReview_ShouldReturnAllPendingVersions()
        {
            IResponseManager repository = new AzureRepositoryMock(InitPluginResponse());
            var pluginRepository = new PluginRepository(repository);
            var pluginVersionRepository = new PluginVersionRepository(pluginRepository);
            var versions = await pluginVersionRepository.GetPluginVersions(0, Status.InReview);

            Assert.Equal(new[]
            {
                new PluginVersion { VersionId = "e04e29da-6429-454c-b4fe-0d831e1e7c56", VersionStatus = Status.InReview },
                new PluginVersion { VersionId = "f32b8f18-0823-49d0-b8c6-21225cafcc81", VersionStatus = Status.InReview }

            }, versions);
            Assert.Equal(2, versions.Count());
        }

        [Fact]
        public async void PluginVersionRepositoryTest_GetAllVersionsWhenStatusIsDraft_ShouldReturnAllDraftVersions()
        {
            IResponseManager repository = new AzureRepositoryMock(InitPluginResponse());
            var pluginRepository = new PluginRepository(repository);
            var pluginVersionRepository = new PluginVersionRepository(pluginRepository);
            var versions = await pluginVersionRepository.GetPluginVersions(0, Status.Draft);

            Assert.Equal(new[]
            {
                new PluginVersion { VersionId = "0f65f21c-dcb1-4648-8b05-38daf657e28d", VersionStatus = Status.Draft },
                new PluginVersion { VersionId = "f32b8f18-0823-49d0-b8c6-21225cafcc81", VersionStatus = Status.Draft }

            }, versions);
            Assert.Equal(2, versions.Count());
        }

        [Fact]
        public async void PluginVersionRepositoryTest_CheckIfAVersionHasActiveChanges_ShouldReturnTrue()
        {
            IResponseManager repository = new AzureRepositoryMock(InitPluginResponse());
            var pluginRepository = new PluginRepository(repository);
            var pluginVersionRepository = new PluginVersionRepository(pluginRepository);

            Assert.True(await pluginVersionRepository.HasActiveChanges(0, "f32b8f18-0823-49d0-b8c6-21225cafcc81"));
        }

        [Fact]
        public async void PluginVersionRepositoryTest_CheckIfAVersionHasActiveChanges_ShouldReturnFalse()
        {
            IResponseManager repository = new AzureRepositoryMock(InitPluginResponse());
            var pluginRepository = new PluginRepository(repository);
            var pluginVersionRepository = new PluginVersionRepository(pluginRepository);

            Assert.False(await pluginVersionRepository.HasActiveChanges(0, "e04e29da-6429-454c-b4fe-0d831e1e7c56"));
        }

        [Fact]
        public async void PluginVersionRepositoryTest_CheckIfAVersionHasPendingChanges_ShouldReturnTrue()
        {
            IResponseManager repository = new AzureRepositoryMock(InitPluginResponse());
            var pluginRepository = new PluginRepository(repository);
            var pluginVersionRepository = new PluginVersionRepository(pluginRepository);

            Assert.True(await pluginVersionRepository.HasPendingChanges(0, "f32b8f18-0823-49d0-b8c6-21225cafcc81", "Developer"));
        }

        [Fact]
        public async void PluginVersionRepositoryTest_CheckIfAVersionHasPendingChanges_ShouldReturnFalse()
        {
            IResponseManager repository = new AzureRepositoryMock(InitPluginResponse());
            var pluginRepository = new PluginRepository(repository);
            var pluginVersionRepository = new PluginVersionRepository(pluginRepository);

            Assert.False(await pluginVersionRepository.HasPendingChanges(0, null, "Developer"));
        }

        [Fact]
        public async void PluginVersionRepositoryTest_CheckIfAVersionHasDraftChanges_ShouldReturnTrue()
        {
            IResponseManager repository = new AzureRepositoryMock(InitPluginResponse());
            var pluginRepository = new PluginRepository(repository);
            var pluginVersionRepository = new PluginVersionRepository(pluginRepository);

            Assert.True(await pluginVersionRepository.HasDraftChanges(0, "f32b8f18-0823-49d0-b8c6-21225cafcc81", "Developer"));
        }

        [Fact]
        public async void PluginVersionRepositoryTest_CheckIfAVersionHasDraftChanges_ShouldReturnFalse()
        {
            IResponseManager repository = new AzureRepositoryMock(InitPluginResponse());
            var pluginRepository = new PluginRepository(repository);
            var pluginVersionRepository = new PluginVersionRepository(pluginRepository);

            Assert.False(await pluginVersionRepository.HasDraftChanges(0, "e04e29da-6429-454c-b4fe-0d831e1e7c56", "Developer"));
        }

        [Fact]
        public async void PluginVersionRepositoryTest_RemovePluginVersion_ShouldRemoveCorrespondingVersion()
        {
            IResponseManager repository = new AzureRepositoryMock(InitPluginResponse());
            var pluginRepository = new PluginRepository(repository);
            var pluginVersionRepository = new PluginVersionRepository(pluginRepository);
            await pluginVersionRepository.RemovePluginVersion(0, "f32b8f18-0823-49d0-b8c6-21225cafcc81");
            var versions = await pluginVersionRepository.GetPluginVersions(0);

            Assert.Equal(new[]
            {
                new PluginVersion { VersionId = "e04e29da-6429-454c-b4fe-0d831e1e7c56", VersionStatus = Status.InReview },
                new PluginVersion { VersionId = "0f65f21c-dcb1-4648-8b05-38daf657e28d", VersionStatus = Status.Draft }
            }, versions);
        }

        [Fact]
        public async void PluginVersionRepositoryTest_RemovePluginVersionWhenIdIsNull_ShouldRemoveCorrespondingVersion()
        {
            IResponseManager repository = new AzureRepositoryMock(InitPluginResponse());
            var pluginRepository = new PluginRepository(repository);
            var pluginVersionRepository = new PluginVersionRepository(pluginRepository);
            await pluginVersionRepository.RemovePluginVersion(0, null);
            var versions = await pluginVersionRepository.GetPluginVersions(0);

            Assert.Equal(new[]
            {
                new PluginVersion { VersionId = "f32b8f18-0823-49d0-b8c6-21225cafcc81", VersionStatus = Status.Active },
                new PluginVersion { VersionId = "e04e29da-6429-454c-b4fe-0d831e1e7c56", VersionStatus = Status.InReview },
                new PluginVersion { VersionId = "0f65f21c-dcb1-4648-8b05-38daf657e28d", VersionStatus = Status.Draft }
            }, versions);
        }

        [Fact]
        public async void PluginVersionRepositoryTest_AddNewVersion_ShouldAddTheNewVersionToRepository()
        {
            IResponseManager repository = new AzureRepositoryMock(InitPluginResponse());
            var pluginRepository = new PluginRepository(repository);
            var pluginVersionRepository = new PluginVersionRepository(pluginRepository);
            await pluginVersionRepository.Save(0, new PluginVersion { VersionId = "6ca0cf92-cd02-4791-bd3c-e1ecd1b83237", VersionStatus = Status.Draft });
            var versions = await pluginVersionRepository.GetPluginVersions(0);

            Assert.Equal(new[]
            {
                new PluginVersion { VersionId = "f32b8f18-0823-49d0-b8c6-21225cafcc81", VersionStatus = Status.Active },
                new PluginVersion { VersionId = "e04e29da-6429-454c-b4fe-0d831e1e7c56", VersionStatus = Status.InReview },
                new PluginVersion { VersionId = "0f65f21c-dcb1-4648-8b05-38daf657e28d", VersionStatus = Status.Draft },
                new PluginVersion { VersionId = "6ca0cf92-cd02-4791-bd3c-e1ecd1b83237", VersionStatus = Status.Draft }
            }, versions);
        }

        [Fact]
        public async void PluginVersionRepositoryTest_UpdateExistingVersion_ShouldUpdateTheVersionFromRepository()
        {
            IResponseManager repository = new AzureRepositoryMock(InitPluginResponse());
            var pluginRepository = new PluginRepository(repository);
            var pluginVersionRepository = new PluginVersionRepository(pluginRepository);
            await pluginVersionRepository.Save(0, new PluginVersion { VersionId = "e04e29da-6429-454c-b4fe-0d831e1e7c56", VersionStatus = Status.Active });
            var versions = await pluginVersionRepository.GetPluginVersions(0);

            Assert.Equal(new[]
            {
                new PluginVersion { VersionId = "f32b8f18-0823-49d0-b8c6-21225cafcc81", VersionStatus = Status.Active },
                new PluginVersion { VersionId = "e04e29da-6429-454c-b4fe-0d831e1e7c56", VersionStatus = Status.Active },
                new PluginVersion { VersionId = "0f65f21c-dcb1-4648-8b05-38daf657e28d", VersionStatus = Status.Draft }
            }, versions);
        }

        [Fact]
        public async void PluginRepositoryTest_CheckIfAVersionExists_ShouldReturnTrue()
        {
            IResponseManager repository = new AzureRepositoryMock(InitPluginResponse());
            var pluginRepository = new PluginRepository(repository);
            var pluginVersionRepository = new PluginVersionRepository(pluginRepository);

            Assert.True(await pluginVersionRepository.ExistsVersion(0, "e04e29da-6429-454c-b4fe-0d831e1e7c56"));
        }

        private static PluginResponse<PluginDetails> InitPluginResponse()
        {
            return new PluginResponse<PluginDetails>
            {
                Value = new List<PluginDetails>
                {
                    new PluginDetails
                    {
                        Id = 0,
                        Versions = new List<PluginVersion>
                        {
                            new PluginVersion { VersionId = "f32b8f18-0823-49d0-b8c6-21225cafcc81", VersionStatus = Status.Active }
                        },
                        Drafts = new List<PluginVersion>
                        {
                            new PluginVersion { VersionId = "0f65f21c-dcb1-4648-8b05-38daf657e28d", VersionStatus = Status.Draft },
                            new PluginVersion { VersionId = "f32b8f18-0823-49d0-b8c6-21225cafcc81", VersionStatus = Status.Draft }
                        },
                        Pending = new List<PluginVersion>
                        {
                            new PluginVersion { VersionId = "e04e29da-6429-454c-b4fe-0d831e1e7c56", VersionStatus = Status.InReview },
                            new PluginVersion { VersionId = "f32b8f18-0823-49d0-b8c6-21225cafcc81", VersionStatus = Status.InReview }
                        },
                    }
                }
            };
        }
    }
}

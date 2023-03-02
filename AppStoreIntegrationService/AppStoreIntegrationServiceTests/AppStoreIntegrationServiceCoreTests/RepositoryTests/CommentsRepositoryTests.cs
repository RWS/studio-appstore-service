using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository;
using AppStoreIntegrationServiceManagement.Model.Repository;
using AppStoreIntegrationServiceTests.AppStoreIntegrationServiceCoreTests.Mock;
using Xunit;

namespace AppStoreIntegrationServiceTests.AppStoreIntegrationServiceCoreTests.RepositoryTests
{
    public class CommentsRepositoryTests
    {
        [Fact]
        public async void CommentsRepositoryTest_GetAllPluginCommentsWhenStorageIsNullOrEmpty_ShouldReturnEmptyList()
        {
            var manager = new AzureRepositoryMock(data: null);
            var repository = new CommentsRepository(manager);

            Assert.Empty(await repository.GetComments(0));
        }

        [Fact]
        public async void CommentsRepositoryTest_GetAllPluginVersionCommentsWhenStorageIsNullOrEmpty_ShouldReturnEmptyList()
        {
            var manager = new AzureRepositoryMock(data: null);
            var repository = new CommentsRepository(manager);

            Assert.Empty(await repository.GetComments(0, Guid.NewGuid().ToString()));
        }

        [Fact]
        public async void CommentsRepositoryTest_GetAllPluginComments_ShouldReturnAllComments()
        {
            var manager = new AzureRepositoryMock(AddCommentsToStorage());
            var repository = new CommentsRepository(manager);

            Assert.Equal(new List<Comment>
            {
                new Comment { CommentDescription = "Test plugin comment 1" },
                new Comment { CommentDescription = "Test plugin comment 2" },
                new Comment { CommentDescription = "Test plugin comment 3" }
            }, await repository.GetComments(0));
        }

        [Fact]
        public async void CommentsRepositoryTest_GetCommentsForInexistentPlugin_ShouldReturnEmptyList()
        {
            var manager = new AzureRepositoryMock(AddCommentsToStorage());
            var repository = new CommentsRepository(manager);

            Assert.Empty(await repository.GetComments(-1));
        }

        [Fact]
        public async void CommentsRepositoryTest_GetCommentsForInexistentVersion_ShouldReturnEmptyList()
        {
            var manager = new AzureRepositoryMock(AddCommentsToStorage());
            var repository = new CommentsRepository(manager);

            Assert.Empty(await repository.GetComments(0, "cd80292a-9dcb-4242-901e-c7fa320eac05"));
        }

        [Fact]
        public async void CommentsRepositoryTest_GetAllPluginVersionComments_ShouldReturnAllComments()
        {
            var manager = new AzureRepositoryMock(AddCommentsToStorage());
            var repository = new CommentsRepository(manager);

            Assert.Equal(new List<Comment>
            {
                new Comment { CommentDescription = "Test version comment 1" },
                new Comment { CommentDescription = "Test version comment 2" },
                new Comment { CommentDescription = "Test version comment 3" }
            }, await repository.GetComments(0, "e60c4c2f-c5c6-4561-8c4e-f9eb8199b1aa"));
        }

        [Fact]
        public async void CommentsRepositoryTest_GetPluginCommentById_ShouldReturnSpecificComment()
        {
            var manager = new AzureRepositoryMock(AddCommentsToStorage());
            var repository = new CommentsRepository(manager);

            Assert.Equal(new Comment { CommentDescription = "Test plugin comment 3" }, await repository.GetComment(0, 3));
        }

        [Fact]
        public async void CommentsRepositoryTest_GetPluginCommentByIdForInexistentPlugin_ShouldReturnNull()
        {
            var manager = new AzureRepositoryMock(AddCommentsToStorage());
            var repository = new CommentsRepository(manager);

            Assert.Null(await repository.GetComment(-1, 3));
        }

        [Fact]
        public async void CommentsRepositoryTest_GetPluginVersionCommentById_ShouldReturnSpecificComment()
        {
            var manager = new AzureRepositoryMock(AddCommentsToStorage());
            var repository = new CommentsRepository(manager);

            Assert.Equal(new Comment { CommentDescription = "Test version comment 3" }, await repository.GetComment(0, 3, "e60c4c2f-c5c6-4561-8c4e-f9eb8199b1aa"));
        }

        [Fact]
        public async void CommentsRepositoryTest_GetPluginVersionCommentByIdForInexistentVersion_ShouldReturnNull()
        {
            var manager = new AzureRepositoryMock(AddCommentsToStorage());
            var repository = new CommentsRepository(manager);

            Assert.Null(await repository.GetComment(-1, 3, "cd80292a-9dcb-4242-901e-c7fa320eac05"));
        }

        [Fact]
        public async void CommentsRepositoryTest_SaveNewPluginComment_CommentShouldBeSaved()
        {
            var manager = new AzureRepositoryMock(AddCommentsToStorage());
            var repository = new CommentsRepository(manager);
            await repository.SaveComment(new Comment 
            { 
                CommentId = 4, 
                CommentDescription = "Test plugin comment 4" 
            }, 0);

            Assert.Equal(new List<Comment>
            {
                new Comment { CommentDescription = "Test plugin comment 1" },
                new Comment { CommentDescription = "Test plugin comment 2" },
                new Comment { CommentDescription = "Test plugin comment 3" },
                new Comment { CommentDescription = "Test plugin comment 4" }
            }, await repository.GetComments(0));

            Assert.Equal(new Comment { CommentDescription = "Test plugin comment 4" }, await repository.GetComment(0, 4));
        }

        [Fact]
        public async void CommentsRepositoryTest_SaveExistentPluginComment_CommentShouldBeUpdated()
        {
            var manager = new AzureRepositoryMock(AddCommentsToStorage());
            var repository = new CommentsRepository(manager);
            await repository.SaveComment(new Comment
            {
                CommentId = 3,
                CommentDescription = "Test plugin comment 3 - updated"
            }, 0);

            Assert.Equal(new List<Comment>
            {
                new Comment { CommentDescription = "Test plugin comment 1" },
                new Comment { CommentDescription = "Test plugin comment 2" },
                new Comment { CommentDescription = "Test plugin comment 3 - updated" },
            }, await repository.GetComments(0));

            Assert.Equal(new Comment { CommentDescription = "Test plugin comment 3 - updated" }, await repository.GetComment(0, 3));
        }

        [Fact]
        public async void CommentsRepositoryTest_SaveNewPluginVersionComment_CommentShouldBeSaved()
        {
            var manager = new AzureRepositoryMock(AddCommentsToStorage());
            var repository = new CommentsRepository(manager);
            await repository.SaveComment(new Comment
            {
                CommentId = 4,
                CommentDescription = "Test version comment 4"
            }, 0, "e60c4c2f-c5c6-4561-8c4e-f9eb8199b1aa");

            Assert.Equal(new List<Comment>
            {
                new Comment { CommentDescription = "Test version comment 1" },
                new Comment { CommentDescription = "Test version comment 2" },
                new Comment { CommentDescription = "Test version comment 3" },
                new Comment { CommentDescription = "Test version comment 4" }
            }, await repository.GetComments(0, "e60c4c2f-c5c6-4561-8c4e-f9eb8199b1aa"));

            Assert.Equal(new Comment { CommentDescription = "Test version comment 4" }, await repository.GetComment(0, 4, "e60c4c2f-c5c6-4561-8c4e-f9eb8199b1aa"));
        }

        [Fact]
        public async void CommentsRepositoryTest_SaveExistentPluginVersionComment_CommentShouldBeUpdated()
        {
            var manager = new AzureRepositoryMock(AddCommentsToStorage());
            var repository = new CommentsRepository(manager);
            await repository.SaveComment(new Comment
            {
                CommentId = 1,
                CommentDescription = "Test version comment 1 - updated"
            }, 0, "e60c4c2f-c5c6-4561-8c4e-f9eb8199b1aa");

            Assert.Equal(new List<Comment>
            {
                new Comment { CommentDescription = "Test version comment 1 - updated" },
                new Comment { CommentDescription = "Test version comment 2" },
                new Comment { CommentDescription = "Test version comment 3" },
            }, await repository.GetComments(0, "e60c4c2f-c5c6-4561-8c4e-f9eb8199b1aa"));

            Assert.Equal(new Comment { CommentDescription = "Test version comment 1 - updated" }, await repository.GetComment(0, 1, "e60c4c2f-c5c6-4561-8c4e-f9eb8199b1aa"));
        }

        [Fact]
        public async void CommentsRepositoryTest_SaveNullComment_CommentShouldSaved()
        {
            var manager = new AzureRepositoryMock(AddCommentsToStorage());
            var repository = new CommentsRepository(manager);
            await repository.SaveComment(null, 0);

            Assert.Equal(new List<Comment>
            {
                new Comment { CommentDescription = "Test plugin comment 1" },
                new Comment { CommentDescription = "Test plugin comment 2" },
                new Comment { CommentDescription = "Test plugin comment 3" },
            }, await repository.GetComments(0));
        }

        [Fact]
        public async void CommentsRepositoryTest_DeletePluginComment_CommentShouldBeDeleted()
        {
            var manager = new AzureRepositoryMock(AddCommentsToStorage());
            var repository = new CommentsRepository(manager);
            await repository.DeleteComment(1, 0);

            Assert.Equal(new List<Comment>
            {
                new Comment { CommentDescription = "Test plugin comment 2" },
                new Comment { CommentDescription = "Test plugin comment 3" },
            }, await repository.GetComments(0));
        }

        [Fact]
        public async void CommentsRepositoryTest_DeleteAllPluginComment_PluginCommentsCollectionShouldBeEmpty()
        {
            var manager = new AzureRepositoryMock(AddCommentsToStorage());
            var repository = new CommentsRepository(manager);
            await repository.DeleteComments(0);

            Assert.Empty(await repository.GetComments(0));
        }

        [Fact]
        public async void CommentsRepositoryTest_DeleteInexistentPluginComment_CommentsCollectionShouldNotBeChanged()
        {
            var manager = new AzureRepositoryMock(AddCommentsToStorage());
            var repository = new CommentsRepository(manager);
            await repository.DeleteComment(4, 0);

            Assert.Equal(new List<Comment>
            {
                new Comment { CommentDescription = "Test plugin comment 1" },
                new Comment { CommentDescription = "Test plugin comment 2" },
                new Comment { CommentDescription = "Test plugin comment 3" },
            }, await repository.GetComments(0));
        }

        [Fact]
        public async void CommentsRepositoryTest_DeletePluginVersionComment_CommentShouldBeDeleted()
        {
            var manager = new AzureRepositoryMock(AddCommentsToStorage());
            var repository = new CommentsRepository(manager);
            await repository.DeleteComment(3, 0, "e60c4c2f-c5c6-4561-8c4e-f9eb8199b1aa");

            Assert.Equal(new List<Comment>
            {
                new Comment { CommentDescription = "Test version comment 1" },
                new Comment { CommentDescription = "Test version comment 2" },
            }, await repository.GetComments(0, "e60c4c2f-c5c6-4561-8c4e-f9eb8199b1aa"));
        }

        [Fact]
        public async void CommentsRepositoryTest_DeleteAllPluginVersionComment_PluginVersionCommentsCollectionShouldBeEmpty()
        {
            var manager = new AzureRepositoryMock(AddCommentsToStorage());
            var repository = new CommentsRepository(manager);
            await repository.DeleteComments(0, "e60c4c2f-c5c6-4561-8c4e-f9eb8199b1aa");

            Assert.Empty(await repository.GetComments(0, "e60c4c2f-c5c6-4561-8c4e-f9eb8199b1aa"));
        }

        private static PluginResponse<PluginDetails> AddCommentsToStorage()
        {
            return new PluginResponse<PluginDetails>
            {
                Comments = new Dictionary<int, CommentPackage>
                {
                    [0] = new CommentPackage
                    {
                        PluginComments = new List<Comment>
                        {
                            new Comment { CommentId = 1, CommentDescription = "Test plugin comment 1" },
                            new Comment { CommentId = 2, CommentDescription = "Test plugin comment 2" },
                            new Comment { CommentId = 3, CommentDescription = "Test plugin comment 3" }
                        },
                        VersionComments = new Dictionary<string, IEnumerable<Comment>>
                        {
                            ["e60c4c2f-c5c6-4561-8c4e-f9eb8199b1aa"] = new List<Comment>
                            {
                                new Comment { CommentId = 1, CommentDescription = "Test version comment 1" },
                                new Comment { CommentId = 2, CommentDescription = "Test version comment 2" },
                                new Comment { CommentId = 3, CommentDescription = "Test version comment 3" }
                            }
                        }
                    }
                }
            };
        }
    }
}

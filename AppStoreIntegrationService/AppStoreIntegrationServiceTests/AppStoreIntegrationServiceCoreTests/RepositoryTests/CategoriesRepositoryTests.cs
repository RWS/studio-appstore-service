using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository;
using AppStoreIntegrationServiceCore.Repository.Interface;
using AppStoreIntegrationServiceTests.AppStoreIntegrationServiceCoreTests.Mock;
using Xunit;

namespace AppStoreIntegrationServiceTests.AppStoreIntegrationServiceCoreTests.RepositoryTests
{
    public class CategoriesRepositoryTests
    {
        [Fact]
        public async void CategoryRepositoryTest_GetAllCategories_ShouldReturnAllCategoriesFromStorage()
        {
            IResponseManager repository = new AzureRepositoryMock(new PluginResponse<PluginDetails>
            {
                Categories = new List<CategoryDetails>
                {
                    new CategoryDetails { Id = "1", Name = "Test 1" },
                    new CategoryDetails { Id = "2", Name = "Test 2" }
                }
            });

            var categoryRepository = new CategoriesRepository(repository);
            var categories = await categoryRepository.GetAllCategories();

            Assert.Equal(new List<CategoryDetails>
            {
                new CategoryDetails { Id = "1", Name = "Test 1" },
                new CategoryDetails { Id = "2", Name = "Test 2" }
            }, categories);

            Assert.Equal(2, categories.Count());
        }

        [Fact]
        public async void CategoryRepositoryTest_SaveCategories_CategoriesShouldBeSavedToStorage()
        {
            IResponseManager repository = new AzureRepositoryMock();

            var categoryRepository = new CategoriesRepository(repository);
            await categoryRepository.SaveCategories(new List<CategoryDetails>
            {
                new CategoryDetails { Id = "1", Name = "Test 1" },
                new CategoryDetails { Id = "2", Name = "Test 2" },
                new CategoryDetails { Id = "3", Name = "Test 3" },
                new CategoryDetails { Id = "4", Name = "Test 4" }
            });

            var categories = await categoryRepository.GetAllCategories();

            Assert.Equal(new List<CategoryDetails>
            {
                new CategoryDetails { Id = "1", Name = "Test 1" },
                new CategoryDetails { Id = "2", Name = "Test 2" },
                new CategoryDetails { Id = "3", Name = "Test 3" },
                new CategoryDetails { Id = "4", Name = "Test 4" }
            }, categories);

            Assert.Equal(4, categories.Count());
        }

        [Fact]
        public async void CategoryRepositoryTest_GetCategoryByExistentId_ShouldReturnTheExpectedCategory()
        {
            IResponseManager repository = new AzureRepositoryMock(new PluginResponse<PluginDetails>
            {
                Categories = new List<CategoryDetails>
                {
                    new CategoryDetails { Id = "1", Name = "Test 1" },
                    new CategoryDetails { Id = "2", Name = "Test 2" }
                }
            });

            var categoryRepository = new CategoriesRepository(repository);
            Assert.Equal(new CategoryDetails { Id = "1", Name = "Test 1" }, await categoryRepository.GetCategoryById("1"));
        }

        [Fact]
        public async void CategoryRepositoryTest_GetCategoryByInexistentId_ShouldReturnNull()
        {
            IResponseManager repository = new AzureRepositoryMock(new PluginResponse<PluginDetails>
            {
                Categories = new List<CategoryDetails>
                {
                    new CategoryDetails { Id = "1", Name = "Test 1" },
                    new CategoryDetails { Id = "2", Name = "Test 2" }
                }
            });

            var categoryRepository = new CategoriesRepository(repository);
            Assert.Null(await categoryRepository.GetCategoryById("10"));
        }

        [Fact]
        public async void CategoryRepositoryTest_GetCategoryByNullId_ShouldReturnNull()
        {
            IResponseManager repository = new AzureRepositoryMock(new PluginResponse<PluginDetails>
            {
                Categories = new List<CategoryDetails>
                {
                    new CategoryDetails { Id = "1", Name = "Test 1" },
                    new CategoryDetails { Id = "2", Name = "Test 2" }
                }
            });

            var categoryRepository = new CategoriesRepository(repository);
            Assert.Null(await categoryRepository.GetCategoryById(null));
        }

        [Fact]
        public async void CategoryRepositoryTest_UpdateCategory_TheCorrespondingCategoryShouldBeUpdated()
        {
            IResponseManager repository = new AzureRepositoryMock(new PluginResponse<PluginDetails>
            {
                Categories = new List<CategoryDetails>
                {
                    new CategoryDetails { Id = "1", Name = "Test 1" },
                    new CategoryDetails { Id = "2", Name = "Test 2" }
                }
            });

            var categoryRepository = new CategoriesRepository(repository);
            Assert.True(await categoryRepository.TryUpdateCategory(new CategoryDetails
            {
                Id = "1",
                Name = "Test 1 updated"
            }));
            Assert.Equal(new CategoryDetails { Id = "1", Name = "Test 1 updated" }, await categoryRepository.GetCategoryById("1"));
        }

        [Fact]
        public async void CategoryRepositoryTest_UpdateCategoryWithTheSameNameAndDifferentId_CategoryShouldNotBeSaved()
        {
            IResponseManager repository = new AzureRepositoryMock(new PluginResponse<PluginDetails>
            {
                Categories = new List<CategoryDetails>
                {
                    new CategoryDetails { Id = "1", Name = "Test 1" },
                    new CategoryDetails { Id = "2", Name = "Test 2" }
                }
            });

            var categoryRepository = new CategoriesRepository(repository);
            Assert.False(await categoryRepository.TryUpdateCategory(new CategoryDetails
            {
                Id = "5",
                Name = "Test 1"
            }));
            Assert.Null(await categoryRepository.GetCategoryById("5"));
        }

        [Fact]
        public async void CategoryRepositoryTest_UpdateNullCategory_CategoryShouldNotBeSaved()
        {
            IResponseManager repository = new AzureRepositoryMock(new PluginResponse<PluginDetails>
            {
                Categories = new List<CategoryDetails>
                {
                    new CategoryDetails { Id = "1", Name = "Test 1" },
                    new CategoryDetails { Id = "2", Name = "Test 2" }
                }
            });

            var categoryRepository = new CategoriesRepository(repository);
            Assert.False(await categoryRepository.TryUpdateCategory(null));
        }

        [Fact]
        public async void CategoryRepositoryTest_UpdateCategoryWhenCategoriesStorageIsEmpty_CategoryShouldBeSaved()
        {
            IResponseManager repository = new AzureRepositoryMock();
            var categoryRepository = new CategoriesRepository(repository);
            Assert.True(await categoryRepository.TryUpdateCategory(new CategoryDetails { Id = "1", Name = "Test 1" }));
            Assert.Equal(new CategoryDetails { Id = "1", Name = "Test 1" }, await categoryRepository.GetCategoryById("1"));
        }

        [Fact]
        public async void CategoryRepositoryTest_DeleteCategory_CategoryShouldBeRemoved()
        {
            IResponseManager repository = new AzureRepositoryMock(new PluginResponse<PluginDetails>
            {
                Categories = new List<CategoryDetails>
                {
                    new CategoryDetails { Id = "1", Name = "Test 1" },
                    new CategoryDetails { Id = "2", Name = "Test 2" },
                    new CategoryDetails { Id = "3", Name = "Test 3" }
                }
            });

            var categoryRepository = new CategoriesRepository(repository);
            await categoryRepository.DeleteCategory("2");
            Assert.Equal(new List<CategoryDetails>
            {
                new CategoryDetails { Id = "1", Name = "Test 1" },
                new CategoryDetails { Id = "3", Name = "Test 3" }
            }, await categoryRepository.GetAllCategories());
        }

        [Fact]
        public async void CategoryRepositoryTest_DeleteInexistentCategory_CategoriesShouldNotBeChanged()
        {
            IResponseManager repository = new AzureRepositoryMock(new PluginResponse<PluginDetails>
            {
                Categories = new List<CategoryDetails>
                {
                    new CategoryDetails { Id = "1", Name = "Test 1" },
                    new CategoryDetails { Id = "2", Name = "Test 2" },
                    new CategoryDetails { Id = "3", Name = "Test 3" }
                }
            });

            var categoryRepository = new CategoriesRepository(repository);
            await categoryRepository.DeleteCategory("5");

            Assert.Equal(new List<CategoryDetails>
            {
                new CategoryDetails { Id = "1", Name = "Test 1" },
                new CategoryDetails { Id = "2", Name = "Test 2" },
                new CategoryDetails { Id = "3", Name = "Test 3" }
            }, await categoryRepository.GetAllCategories());
        }

        [Fact]
        public async void CategoryRepositoryTest_DeleteCategoryWhenIdIsNull_CategoriesShouldNotBeChanged()
        {
            IResponseManager repository = new AzureRepositoryMock(new PluginResponse<PluginDetails>
            {
                Categories = new List<CategoryDetails>
                {
                    new CategoryDetails { Id = "1", Name = "Test 1" },
                    new CategoryDetails { Id = "2", Name = "Test 2" },
                    new CategoryDetails { Id = "3", Name = "Test 3" }
                }
            });

            var categoryRepository = new CategoriesRepository(repository);
            await categoryRepository.DeleteCategory(null);

            Assert.Equal(new List<CategoryDetails>
            {
                new CategoryDetails { Id = "1", Name = "Test 1" },
                new CategoryDetails { Id = "2", Name = "Test 2" },
                new CategoryDetails { Id = "3", Name = "Test 3" }
            }, await categoryRepository.GetAllCategories());
        }

        [Fact]
        public async void CategoryRepositoryTest_DeleteCategoryWhenStorageIsEmpty_TheStorageRemainsEmpty()
        {
            IResponseManager repository = new AzureRepositoryMock();
            var categoryRepository = new CategoriesRepository(repository);
            await categoryRepository.DeleteCategory("1");
            Assert.Empty(await categoryRepository.GetAllCategories());
        }
    }
}

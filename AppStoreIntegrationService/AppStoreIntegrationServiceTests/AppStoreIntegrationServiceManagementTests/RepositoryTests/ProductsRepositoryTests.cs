using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceManagement.Repository;
using Xunit;
using AppStoreIntegrationServiceCore.Repository.Interface;
using AppStoreIntegrationServiceTests.AppStoreIntegrationServiceManagementTests.Mock;

namespace AppStoreIntegrationServiceTests.AppStoreIntegrationServiceManagementTests.RepositoryTests
{
    public class ProductsRepositoryTests
    {
        [Fact]
        public async void ProductsRepositoryTest_GetAllProducts_ShouldReturnAllProductsFromRepository()
        {
            IResponseManager repository = new AzureRepositoryMock(InitPluginResponse());
            var pluginRepository = new PluginRepository(repository);
            var productsRepository = new ProductsRepository(repository, pluginRepository);
            var products = await productsRepository.GetAllProducts();

            Assert.Equal(new[]
            {
                new ProductDetails { Id = "0", ProductName = "Trados Studio 2021" },
                new ProductDetails { Id = "1", ProductName = "Trados Studio 2022" }
            }, products);
        }

        [Fact]
        public async void ProductsRepositoryTest_GetProductById_ShouldReturnTheCorrespondingProduct()
        {
            IResponseManager repository = new AzureRepositoryMock(InitPluginResponse());
            var pluginRepository = new PluginRepository(repository);
            var productsRepository = new ProductsRepository(repository, pluginRepository);
            var product = await productsRepository.GetProductById("0");

            Assert.Equal(new ProductDetails { Id = "0", ProductName = "Trados Studio 2021" }, product);
        }

        [Fact]
        public async void ProductsRepositoryTest_GetParentById_ShouldReturnTheCorrespondingProduct()
        {
            IResponseManager repository = new AzureRepositoryMock(InitPluginResponse());
            var pluginRepository = new PluginRepository(repository);
            var productsRepository = new ProductsRepository(repository, pluginRepository);
            var product = await productsRepository.GetParentById("0");

            Assert.Equal(new ParentProduct { Id = "0", ProductName = "Trados Studio" }, product);
        }

        [Fact]
        public async void ProductsRepositoryTest_GetAllParentProducts_ShouldReturnAllParentsFromRepository()
        {
            IResponseManager repository = new AzureRepositoryMock(InitPluginResponse());
            var pluginRepository = new PluginRepository(repository);
            var productsRepository = new ProductsRepository(repository, pluginRepository);
            var parents = await productsRepository.GetAllParents();

            Assert.Equal(new[]
            {
                new ParentProduct { Id = "0", ProductName = "Trados Studio" },
                new ParentProduct { Id = "1", ProductName = "Multiterm" }
            }, parents);
        }

        [Fact]
        public async void ProductsRepositoryTest_AddProduct_ShouldAddTheCorrespondingProduct()
        {
            IResponseManager repository = new AzureRepositoryMock(InitPluginResponse());
            var pluginRepository = new PluginRepository(repository);
            var productsRepository = new ProductsRepository(repository, pluginRepository);
            await productsRepository.TryUpdateProduct(new ProductDetails { Id = "2", ProductName = "Trados Studio 2017" });
            var products = await productsRepository.GetAllProducts();

            Assert.Equal(new[]
            {
                new ProductDetails { Id = "0", ProductName = "Trados Studio 2021" },
                new ProductDetails { Id = "1", ProductName = "Trados Studio 2022" },
                new ProductDetails { Id = "2", ProductName = "Trados Studio 2017" }
            }, products);
        }

        [Fact]
        public async void ProductsRepositoryTest_UpdateProduct_ShouldUpdateTheCorrespondingProduct()
        {
            IResponseManager repository = new AzureRepositoryMock(InitPluginResponse());
            var pluginRepository = new PluginRepository(repository);
            var productsRepository = new ProductsRepository(repository, pluginRepository);
            await productsRepository.TryUpdateProduct(new ProductDetails { Id = "1", ProductName = "Trados Studio 2022 (CU6)" });
            var products = await productsRepository.GetAllProducts();

            Assert.Equal(new[]
            {
                new ProductDetails { Id = "0", ProductName = "Trados Studio 2021" },
                new ProductDetails { Id = "1", ProductName = "Trados Studio 2022 (CU6)" }
            }, products);
        }

        [Fact]
        public async void ProductsRepositoryTest_AddParentProduct_ShouldAddTheCorrespondingProduct()
        {
            IResponseManager repository = new AzureRepositoryMock(InitPluginResponse());
            var pluginRepository = new PluginRepository(repository);
            var productsRepository = new ProductsRepository(repository, pluginRepository);
            await productsRepository.TryUpdateProduct(new ParentProduct { Id = "2", ProductName = "Language Cloud" });
            var parents = await productsRepository.GetAllParents();

            Assert.Equal(new[]
            {
                new ParentProduct { Id = "0", ProductName = "Trados Studio" },
                new ParentProduct { Id = "1", ProductName = "Multiterm" },
                new ParentProduct { Id = "2", ProductName = "Language Cloud" }
            }, parents);
        }

        [Fact]
        public async void ProductsRepositoryTest_UpdateParentProduct_ShouldUpdateTheCorrespondingProduct()
        {
            IResponseManager repository = new AzureRepositoryMock(InitPluginResponse());
            var pluginRepository = new PluginRepository(repository);
            var productsRepository = new ProductsRepository(repository, pluginRepository);
            await productsRepository.TryUpdateProduct(new ParentProduct { Id = "1", ProductName = "Multiterm updated" });
            var parents = await productsRepository.GetAllParents();

            Assert.Equal(new[]
            {
                new ParentProduct { Id = "0", ProductName = "Trados Studio" },
                new ParentProduct { Id = "1", ProductName = "Multiterm updated" }
            }, parents);
        }

        [Fact]
        public async void ProductsRepositoryTest_UpdateParentProductWhenIdIsChanged_TheCollectionIsNotChanged()
        {
            IResponseManager repository = new AzureRepositoryMock(InitPluginResponse());
            var pluginRepository = new PluginRepository(repository);
            var productsRepository = new ProductsRepository(repository, pluginRepository);

            Assert.False(await productsRepository.TryUpdateProduct(new ParentProduct { Id = "3", ProductName = "Multiterm" }));

            var parents = await productsRepository.GetAllParents();

            Assert.Equal(new[]
            {
                new ParentProduct { Id = "0", ProductName = "Trados Studio" },
                new ParentProduct { Id = "1", ProductName = "Multiterm" }
            }, parents);
        }

        [Fact]
        public async void ProductsRepositoryTest_RemoveProduct_TheCorrespondingProductShouldBeRemoved()
        {
            IResponseManager repository = new AzureRepositoryMock(InitPluginResponse());
            var pluginRepository = new PluginRepository(repository);
            var productsRepository = new ProductsRepository(repository, pluginRepository);
            await productsRepository.TryDeleteProduct("1");
            var products = await productsRepository.GetAllProducts();

            Assert.Equal(new[]
            {
                new ProductDetails { Id = "0", ProductName = "Trados Studio 2021", ParentProductID = "0" }
            }, products);
        }

        [Fact]
        public async void ProductsRepositoryTest_RemoveParentProduct_TheCorrespondingParentIsRemoved()
        {
            IResponseManager repository = new AzureRepositoryMock(InitPluginResponse());
            var pluginRepository = new PluginRepository(repository);
            var productsRepository = new ProductsRepository(repository, pluginRepository);
            await productsRepository.TryDeleteParent("1");
            var parents = await productsRepository.GetAllParents();

            Assert.Equal(new[]
            {
                new ParentProduct { Id = "0", ProductName = "Trados Studio" }
            }, parents);
        }

        [Fact]
        public async void ProductsRepositoryTest_RemoveAProductInUse_TheCollectionIsUnchanged()
        {
            IResponseManager repository = new AzureRepositoryMock(InitPluginResponse());
            var pluginRepository = new PluginRepository(repository);
            var productsRepository = new ProductsRepository(repository, pluginRepository);
            await productsRepository.TryDeleteProduct("0");
            var products = await productsRepository.GetAllProducts();

            Assert.Equal(new[]
            {
                new ProductDetails { Id = "0", ProductName = "Trados Studio 2021" },
                new ProductDetails { Id = "1", ProductName = "Trados Studio 2022" }
            }, products);
        }

        [Fact]
        public async void ProductsRepositoryTest_RemoveAParentInUse_TheCollectionIsUnchanged()
        {
            IResponseManager repository = new AzureRepositoryMock(InitPluginResponse());
            var pluginRepository = new PluginRepository(repository);
            var productsRepository = new ProductsRepository(repository, pluginRepository);
            await productsRepository.TryDeleteParent("0");
            var parents = await productsRepository.GetAllParents();

            Assert.Equal(new[]
            {
                new ParentProduct { Id = "0", ProductName = "Trados Studio" },
                new ParentProduct { Id = "1", ProductName = "Multiterm" }
            }, parents);
        }

        private static PluginResponse<PluginDetails> InitPluginResponse()
        {
            return new PluginResponse<PluginDetails>
            {
                Products = new[]
                {
                    new ProductDetails { Id = "0", ProductName = "Trados Studio 2021", ParentProductID = "0" },
                    new ProductDetails { Id = "1", ProductName = "Trados Studio 2022", ParentProductID = "0" }
                },
                ParentProducts = new[]
                {
                    new ParentProduct { Id = "0", ProductName = "Trados Studio" },
                    new ParentProduct { Id = "1", ProductName = "Multiterm" }
                },
                Value = new[]
                {
                    new PluginDetails
                    {
                        Versions = new List<PluginVersion>
                        {
                            new PluginVersion
                            {
                                SupportedProducts = new List<string> { "0" }
                            }
                        }
                    }
                }
            };
        }
    }
}

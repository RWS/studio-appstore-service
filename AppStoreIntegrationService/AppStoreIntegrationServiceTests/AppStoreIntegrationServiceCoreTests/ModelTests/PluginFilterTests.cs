using AppStoreIntegrationServiceCore.Model;
using Xunit;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceTests.AppStoreIntegrationServiceCoreTests.Model
{
    public class PluginFilterTests
    {
        [Fact]
        public void FilterPluginsTest_InputPluginsListIsNullOrEmpty_ReturnsAnEmptyList()
        {
            Assert.Empty(PluginFilter.FilterPlugins(null, null, null, null));
            Assert.Empty(PluginFilter.FilterPlugins(Enumerable.Empty<PluginDetails>(), null, null, null));
        }

        [Fact]
        public void FilterPluginsTest_FilterObjectIsNull_ReturnsTheInputList()
        {
            var list = new List<PluginDetails>
            {
                new PluginDetails { Name = "Test" },
                new PluginDetails { Name = "Test 2" }
            };

            Assert.Equal(new List<PluginDetails>
            {
                new PluginDetails { Name = "Test" },
                new PluginDetails { Name = "Test 2" }
            }, PluginFilter.FilterPlugins(list, null, null, null));
        }

        [Fact]
        public void FilterPluginsTest_FilterObjectAndInputListAreNull_ReturnsAnEmptyList()
        {
            Assert.Empty(PluginFilter.FilterPlugins(null, null, null, null));
        }

        [Fact]
        public void FilterPluginsTest_PluginFilterStatusIsActive_ReturnsThePluginsWithStatusActive()
        {
            IEnumerable<PluginDetails> list = new List<PluginDetails>
            {
                new PluginDetails { Name = "Test 1", Status = Status.Active },
                new PluginDetails { Name = "Test 2", Status = Status.Inactive },
                new PluginDetails { Name = "Test 3", Status = Status.Draft },
                new PluginDetails { Name = "Test 4", Status = Status.InReview }
            };

            list = PluginFilter.FilterPlugins(list, new PluginFilter { Status = Status.Active }, null, null);
            Assert.Equal(new List<PluginDetails> { new PluginDetails { Name = "Test 1", Status = Status.Active } }, list);
        }

        [Fact]
        public void FilterPluginsTest_PluginFilterStatusEqualsInactive_ReturnsThePluginsWithStatusInactive()
        {
            IEnumerable<PluginDetails> list = new List<PluginDetails>
            {
                new PluginDetails { Name = "Test 1", Status = Status.Active },
                new PluginDetails { Name = "Test 2", Status = Status.Inactive },
                new PluginDetails { Name = "Test 3", Status = Status.Draft },
                new PluginDetails { Name = "Test 4", Status = Status.InReview }
            };

            list = PluginFilter.FilterPlugins(list, new PluginFilter { Status = Status.Inactive }, null, null);
            Assert.Equal(new List<PluginDetails> { new PluginDetails { Name = "Test 2", Status = Status.Inactive } }, list);
        }

        [Fact]
        public void FilterPluginsTest_PluginFilterStatusEqualsInReview_ReturnsThePluginsWithStatusInReview()
        {
            IEnumerable<PluginDetails> list = new List<PluginDetails>
            {
                new PluginDetails { Name = "Test 1", Status = Status.Active },
                new PluginDetails { Name = "Test 2", Status = Status.Inactive },
                new PluginDetails { Name = "Test 3", Status = Status.Draft },
                new PluginDetails { Name = "Test 4", Status = Status.InReview }
            };

            list = PluginFilter.FilterPlugins(list, new PluginFilter { Status = Status.InReview }, null, null);
            Assert.Equal(new List<PluginDetails> { new PluginDetails { Name = "Test 4", Status = Status.InReview } }, list);
        }

        [Fact]
        public void FilterPluginsTest_PluginFilterStatusEqualsDraft_ReturnsThePluginsWithStatusDraft()
        {
            IEnumerable<PluginDetails> list = new List<PluginDetails>
            {
                new PluginDetails { Name = "Test 1", Status = Status.Active },
                new PluginDetails { Name = "Test 2", Status = Status.Inactive },
                new PluginDetails { Name = "Test 3", Status = Status.Draft },
                new PluginDetails { Name = "Test 4", Status = Status.InReview }
            };

            list = PluginFilter.FilterPlugins(list, new PluginFilter { Status = Status.Draft }, null, null);
            Assert.Equal(new List<PluginDetails> { new PluginDetails { Name = "Test 3", Status = Status.Draft } }, list);
        }

        [Fact]
        public void FilterPluginsTest_PluginFilterStatusEqualsDefault_ReturnsTheInputPluginsList()
        {
            IEnumerable<PluginDetails> list = new List<PluginDetails>
            {
                new PluginDetails { Name = "Test 1", Status = Status.Active },
                new PluginDetails { Name = "Test 2", Status = Status.Inactive },
                new PluginDetails { Name = "Test 3", Status = Status.Draft },
                new PluginDetails { Name = "Test 4", Status = Status.InReview }
            };

            list = PluginFilter.FilterPlugins(list, new PluginFilter { Status = Status.All }, null, null);
            Assert.Equal(new List<PluginDetails> {
                new PluginDetails { Name = "Test 1", Status = Status.Active },
                new PluginDetails { Name = "Test 2", Status = Status.Inactive },
                new PluginDetails { Name = "Test 3", Status = Status.Draft },
                new PluginDetails { Name = "Test 4", Status = Status.InReview }
            }, list);
        }

        [Fact]
        public void FilterPluginsTest_FilterByProduct_ReturnsTheListOfPluginsSupportingTheCorrespondingProduct()
        {
            IEnumerable<PluginDetails> list = new List<PluginDetails>
            {
                new PluginDetails { Name = "Test 1", Versions = new List<PluginVersion> { new PluginVersion { SupportedProducts = new List<string> { "2" } } } },
                new PluginDetails { Name = "Test 2", Versions = new List<PluginVersion> { new PluginVersion { SupportedProducts = new List<string> { "1" } } } },
                new PluginDetails { Name = "Test 3", Versions = new List<PluginVersion> { new PluginVersion { SupportedProducts = new List<string> { "3" } } } },
                new PluginDetails { Name = "Test 4", Versions = new List<PluginVersion> { new PluginVersion { SupportedProducts = new List<string> { "2" } } } }
            };

            list = PluginFilter.FilterPlugins(list, new PluginFilter { SupportedProduct = "2" }, null, null);
            Assert.Equal(new List<PluginDetails> {
                new PluginDetails { Name = "Test 1", Versions = new List<PluginVersion> { new PluginVersion { SupportedProducts = new List<string> { "2" } } } },
                new PluginDetails { Name = "Test 4", Versions = new List<PluginVersion> { new PluginVersion { SupportedProducts = new List<string> { "2" } } } }
            }, list);
        }

        [Fact]
        public void FilterPluginsTest_FilterByANullProduct_ReturnsTheInputList()
        {
            IEnumerable<PluginDetails> list = new List<PluginDetails>
            {
                new PluginDetails { Name = "Test 1" },
                new PluginDetails { Name = "Test 2" }
            };

            list = PluginFilter.FilterPlugins(list, new PluginFilter { SupportedProduct = string.Empty }, null, null);
            Assert.Equal(new List<PluginDetails> {
                new PluginDetails { Name = "Test 1" },
                new PluginDetails { Name = "Test 2" }
            }, list);
        }

        [Fact]
        public void FilterPluginsTest_FilterByAnEmptyProduct_ReturnsTheInputList()
        {
            IEnumerable<PluginDetails> list = new List<PluginDetails>
            {
                new PluginDetails { Name = "Test 1" },
                new PluginDetails { Name = "Test 2" }
            };

            list = PluginFilter.FilterPlugins(list, new PluginFilter { SupportedProduct = null }, null, null);
            Assert.Equal(new List<PluginDetails> {
                new PluginDetails { Name = "Test 1" },
                new PluginDetails { Name = "Test 2" }
            }, list);
        }

        [Fact]
        public void FilterPluginsTest_FilterByQuery_ReturnsTheListOfPluginsWithTheNameMatchingTheQuery()
        {
            IEnumerable<PluginDetails> list = new List<PluginDetails>
            {
                new PluginDetails { Name = "YourProductivity" },
                new PluginDetails { Name = "Language Weaver" },
                new PluginDetails { Name = "Test 3" },
                new PluginDetails { Name = "CleanUp Tasks" }
            };

            list = PluginFilter.FilterPlugins(list, new PluginFilter { Query = "ea" }, null, null);
            Assert.Equal(new List<PluginDetails> {
                new PluginDetails { Name = "Language Weaver" },
                new PluginDetails { Name = "CleanUp Tasks" }
            }, list);

            Assert.Empty(PluginFilter.FilterPlugins(list, new PluginFilter { Query = "xyz" }, null, null));
        }

        [Fact]
        public void FilterPluginsTest_FilterByPaidPrice_ReturnsTheListOfPluginsThatMatchThePricing()
        {
            IEnumerable<PluginDetails> list = new List<PluginDetails>
            {
                new PluginDetails { Name = "Test 1", PaidFor = true },
                new PluginDetails { Name = "Test 2", PaidFor = false },
                new PluginDetails { Name = "Test 3", PaidFor = false },
                new PluginDetails { Name = "Test 4", PaidFor = true }
            };

            list = PluginFilter.FilterPlugins(list, new PluginFilter { Price = "paid" }, null, null);
            Assert.Equal(new List<PluginDetails> {
                new PluginDetails { Name = "Test 1", PaidFor = true },
                new PluginDetails { Name = "Test 4", PaidFor = true }
            }, list);
        }

        [Fact]
        public void FilterPluginsTest_FilterByFreePrice_ReturnsTheListOfPluginsThatMatchThePricing()
        {
            IEnumerable<PluginDetails> list = new List<PluginDetails>
            {
                new PluginDetails { Name = "Test 1", PaidFor = true },
                new PluginDetails { Name = "Test 2", PaidFor = false },
                new PluginDetails { Name = "Test 3", PaidFor = false },
                new PluginDetails { Name = "Test 4", PaidFor = true }
            };

            list = PluginFilter.FilterPlugins(list, new PluginFilter { Price = "free" }, null, null);
            Assert.Equal(new List<PluginDetails> {
                new PluginDetails { Name = "Test 2", PaidFor = false },
                new PluginDetails { Name = "Test 3", PaidFor = false }
            }, list);
        }

        [Fact]
        public void FilterPluginsTest_FilterByCategory_ReturnsThePluginsThatMatchAtLeastACategory()
        {
            IEnumerable<PluginDetails> list = new List<PluginDetails>
            {
                new PluginDetails { Name = "Test 1", Categories = new List<string> { "1", "2" } },
                new PluginDetails { Name = "Test 2", Categories = new List<string> { "3", "4" } },
                new PluginDetails { Name = "Test 3", Categories = new List<string> { "2", "6" } },
                new PluginDetails { Name = "Test 4", Categories = new List<string> { "1", "5" } }
            };

            list = PluginFilter.FilterPlugins(list, new PluginFilter { CategoryId = new List<int> { 2, 5 } }, null, null);
            Assert.Equal(new List<PluginDetails> {
                new PluginDetails { Name = "Test 1", Categories = new List<string> { "1", "2" } },
                new PluginDetails { Name = "Test 3", Categories = new List<string> { "2", "6" } },
                new PluginDetails { Name = "Test 4", Categories = new List<string> { "1", "5" } }
            }, list);
        }

        [Fact]
        public void FilterPluginsTest_FilterByInexistentCategories_ReturnsAnEmptyPluginList()
        {
            IEnumerable<PluginDetails> list = new List<PluginDetails>
            {
                new PluginDetails { Name = "Test 1", Categories = new List<string> { "1", "2" } },
                new PluginDetails { Name = "Test 2", Categories = new List<string> { "3", "4" } },
                new PluginDetails { Name = "Test 3", Categories = new List<string> { "2", "6" } },
                new PluginDetails { Name = "Test 4", Categories = new List<string> { "1", "5" } }
            };

            Assert.Empty(PluginFilter.FilterPlugins(list, new PluginFilter { CategoryId = new List<int> { 9, 10 } }, null, null));
        }

        [Fact]
        public void FilterPluginsTest_FilterByExistingSupportedProductName_ReturnsThePluginsSupportingTheCorrespondingProduct()
        {
            var products = new List<ProductDetails>
            {
                new ProductDetails { ProductName = "SDL Trados Studio 2019", Id = "1" },
                new ProductDetails { ProductName = "SDL Trados Studio 2021", Id = "2" },
                new ProductDetails { ProductName = "SDL Trados Studio 2022", Id = "3" },
            };

            IEnumerable<PluginDetails> list = new List<PluginDetails>
            {
                new PluginDetails { Name = "Test 1", Versions = new List<PluginVersion> { new PluginVersion { SupportedProducts = new List<string> { "2" } } } },
                new PluginDetails { Name = "Test 2", Versions = new List<PluginVersion> { new PluginVersion { SupportedProducts = new List<string> { "1" } } } },
                new PluginDetails { Name = "Test 3", Versions = new List<PluginVersion> { new PluginVersion { SupportedProducts = new List<string> { "3" } } } },
                new PluginDetails { Name = "Test 4", Versions = new List<PluginVersion> { new PluginVersion { SupportedProducts = new List<string> { "2" } } } }
            };

            list = PluginFilter.FilterPlugins(list, new PluginFilter { StudioVersion = "SDL Trados Studio 2021" }, products, null);
            Assert.Equal(new List<PluginDetails> {
                new PluginDetails { Name = "Test 1", Versions = new List<PluginVersion> { new PluginVersion { SupportedProducts = new List<string> { "2" } } } },
                new PluginDetails { Name = "Test 4", Versions = new List<PluginVersion> { new PluginVersion { SupportedProducts = new List<string> { "2" } } } }
            }, list);
        }

        [Fact]
        public void FilterPluginsTest_FilterByInexistentSupportedProductName_ReturnsAnEmptyList()
        {
            var products = new List<ProductDetails>
            {
                new ProductDetails { ProductName = "SDL Trados Studio 2019", Id = "1" },
                new ProductDetails { ProductName = "SDL Trados Studio 2021", Id = "2" },
                new ProductDetails { ProductName = "SDL Trados Studio 2022", Id = "3" },
            };

            IEnumerable<PluginDetails> list = new List<PluginDetails>
            {
                new PluginDetails { Name = "Test 1", Versions = new List<PluginVersion> { new PluginVersion { SupportedProducts = new List<string> { "2" } } } },
                new PluginDetails { Name = "Test 2", Versions = new List<PluginVersion> { new PluginVersion { SupportedProducts = new List<string> { "1" } } } },
                new PluginDetails { Name = "Test 3", Versions = new List<PluginVersion> { new PluginVersion { SupportedProducts = new List<string> { "3" } } } },
                new PluginDetails { Name = "Test 4", Versions = new List<PluginVersion> { new PluginVersion { SupportedProducts = new List<string> { "2" } } } }
            };

            Assert.Empty(PluginFilter.FilterPlugins(list, new PluginFilter { StudioVersion = "Wrong product" }, products, null));
        }

        [Fact]
        public void FilterPluginsTest_FilterByInsideBoundariesMinAndMaxStudioVersion_ReturnsThePluginsWithTheirVersionMatchingStudioVersionBoundaries()
        {
            var products = new List<ProductDetails>
            {
                new ProductDetails { ProductName = "SDL Trados Studio 2019", Id = "1", ParentProductID = "1" },
                new ProductDetails { ProductName = "SDL Trados Studio 2021", Id = "2", ParentProductID = "1" },
                new ProductDetails { ProductName = "SDL Trados Studio 2022", Id = "3", ParentProductID = "1" },
            };

            var parents = new List<ParentProduct>
            {
                new ParentProduct { ProductName = "Trados Studio", Id = "1" }
            };

            IEnumerable<PluginDetails> list = new List<PluginDetails>
            {
                new PluginDetails { Name = "Test 1", Versions = new List<PluginVersion> { new PluginVersion { SupportedProducts = new List<string> { "2" }, MinimumRequiredVersionOfStudio = "16.0.0", MaximumRequiredVersionOfStudio = "16.9.0" } } },
                new PluginDetails { Name = "Test 2", Versions = new List<PluginVersion> { new PluginVersion { SupportedProducts = new List<string> { "1" }, MinimumRequiredVersionOfStudio = "15.0.0", MaximumRequiredVersionOfStudio = "15.9.0" } } },
                new PluginDetails { Name = "Test 3", Versions = new List<PluginVersion> { new PluginVersion { SupportedProducts = new List<string> { "3" }, MinimumRequiredVersionOfStudio = "17.0.0", MaximumRequiredVersionOfStudio = "17.9.0" } } },
            };

            list = PluginFilter.FilterPlugins(list, new PluginFilter { StudioVersion = "17.0.4", BaseProduct = "Trados Studio" }, products, parents);
            Assert.Equal(new List<PluginDetails> {
                new PluginDetails { Name = "Test 3", Versions = new List<PluginVersion> { new PluginVersion { SupportedProducts = new List<string> { "3" }, MinimumRequiredVersionOfStudio = "17.0.0", MaximumRequiredVersionOfStudio = "17.9.0" } } },
            }, list);
        }

        [Fact]
        public void FilterPluginsTest_FilterByOutsideBoundariesMinAndMaxStudioVersion_ReturnsAnEmptyList()
        {
            var products = new List<ProductDetails>
            {
                new ProductDetails { ProductName = "SDL Trados Studio 2019", Id = "1", ParentProductID = "1" },
                new ProductDetails { ProductName = "SDL Trados Studio 2021", Id = "2", ParentProductID = "1" },
                new ProductDetails { ProductName = "SDL Trados Studio 2022", Id = "3", ParentProductID = "1" },
            };

            var parents = new List<ParentProduct>
            {
                new ParentProduct { ProductName = "Trados Studio", Id = "1" }
            };

            IEnumerable<PluginDetails> list = new List<PluginDetails>
            {
                new PluginDetails { Name = "Test 1", Versions = new List<PluginVersion> { new PluginVersion { SupportedProducts = new List<string> { "2" }, MinimumRequiredVersionOfStudio = "16.0.0", MaximumRequiredVersionOfStudio = "16.9.0" } } },
                new PluginDetails { Name = "Test 2", Versions = new List<PluginVersion> { new PluginVersion { SupportedProducts = new List<string> { "1" }, MinimumRequiredVersionOfStudio = "15.0.0", MaximumRequiredVersionOfStudio = "15.9.0" } } },
                new PluginDetails { Name = "Test 3", Versions = new List<PluginVersion> { new PluginVersion { SupportedProducts = new List<string> { "3" }, MinimumRequiredVersionOfStudio = "17.0.0", MaximumRequiredVersionOfStudio = "17.9.0" } } },
            };

            Assert.Empty(PluginFilter.FilterPlugins(list, new PluginFilter { StudioVersion = "14.0.4", BaseProduct = "Trados Studio" }, products, parents));
        }

        [Fact]
        public void FilterPluginsTest_FilterByStudioVersionWhenProductListsAreNullOrEmpty_ReturnsAnEmptyList()
        {
            IEnumerable<PluginDetails> list = new List<PluginDetails>
            {
                new PluginDetails { Name = "Test 1", Versions = new List<PluginVersion> { new PluginVersion { SupportedProducts = new List<string> { "2" }, MinimumRequiredVersionOfStudio = "16.0.0", MaximumRequiredVersionOfStudio = "16.9.0" } } },
                new PluginDetails { Name = "Test 2", Versions = new List<PluginVersion> { new PluginVersion { SupportedProducts = new List<string> { "1" }, MinimumRequiredVersionOfStudio = "15.0.0", MaximumRequiredVersionOfStudio = "15.9.0" } } },
                new PluginDetails { Name = "Test 3", Versions = new List<PluginVersion> { new PluginVersion { SupportedProducts = new List<string> { "3" }, MinimumRequiredVersionOfStudio = "17.0.0", MaximumRequiredVersionOfStudio = "17.9.0" } } },
            };

            Assert.Empty(PluginFilter.FilterPlugins(list, new PluginFilter { StudioVersion = "14.0.4", BaseProduct = "Trados Studio" }, null, null));
        }
    }
}

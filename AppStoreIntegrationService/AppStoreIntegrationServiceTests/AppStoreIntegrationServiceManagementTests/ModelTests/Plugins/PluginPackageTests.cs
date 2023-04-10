using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceManagement.Model.Plugins;
using Xunit;

namespace AppStoreIntegrationServiceTests.AppStoreIntegrationServiceManagementTests.ModelTests.Plugins
{
    public class PluginPackageTests
    {
        public static List<object[]> PluginMatches => GetPluginMatches();
        public static List<object[]> VersionMatches => GetVersionMatches();

        [Theory]
        [MemberData(nameof(PluginMatches))]
        public void PluginPackageTest_CheckIfTheManifestMatchesPluginDetails
        (
            PluginDetails plugin,
            bool expectedNameMatch,
            bool expectedAuthorMatch,
            bool expectedFullMatch
        )
        {
            var pluginPackage = new PluginPackage
            {
                PluginName = "Test",
                Author = "Test Author"
            };

            var (isNameMatch, isAuthorMatch) = pluginPackage.CreatePluginMatchLog(plugin, out var isFullMatch);

            Assert.Equal(expectedNameMatch, isNameMatch);
            Assert.Equal(expectedAuthorMatch, isAuthorMatch);
            Assert.Equal(expectedFullMatch, isFullMatch);
            Assert.Equal(new[] { isFullMatch, isNameMatch, isAuthorMatch }.All(x => x), isFullMatch);
        }

        [Theory]
        [MemberData(nameof(VersionMatches))]
        public void PluginPackageTest_CheckIfTheManifestMatchesVersionDetails
        (
            PluginVersion version,
            VersionManifestComparison expectedComparison,
            bool expectedFullMatch
        )
        {
            var pluginPackage = new PluginPackage
            {
                Version = "4.1.1.2",
                RequiredProduct = new RequiredProduct
                {
                    MinimumStudioVersion = "16.0.0",
                    MaximumStudioVersion = "16.9.0"
                }
            };

            var products = new List<ProductDetails>
            {
                new ProductDetails
                {
                    Id = "1",
                    MinimumStudioVersion = "16.0.0",
                }
            };

            var comparison = pluginPackage.CreateVersionMatchLog(version, products, out var isFullMatch);

            Assert.Equal(expectedComparison, comparison);
            Assert.Equal(expectedFullMatch, isFullMatch);
        }

        private static List<object[]> GetVersionMatches()
        {
            return new List<object[]>
            {
                new object[]
                {
                    new PluginVersion
                    {
                        SupportedProducts = new List<string>{ "1" },
                        MinimumRequiredVersionOfStudio = "16.0.0",
                        MaximumRequiredVersionOfStudio = "16.9.0",
                        VersionNumber = "4.1.1.2"
                    },
                    new VersionManifestComparison
                    {
                         IsMaxVersionMatch = true,
                         IsMinVersionMatch = true,
                         IsProductMatch = true,
                         IsVersionMatch = true,
                    },
                    true
                },
                new object[]
                {
                    new PluginVersion
                    {
                        SupportedProducts = new List<string>{ "2" },
                        MinimumRequiredVersionOfStudio = "16.0.0",
                        MaximumRequiredVersionOfStudio = "16.9.0",
                        VersionNumber = "4.1.1.2"
                    },
                    new VersionManifestComparison
                    {
                         IsProductMatch = false,
                         IsMinVersionMatch = true,
                         IsMaxVersionMatch = true,
                         IsVersionMatch = true,
                    },
                    false
                },
                new object[]
                {
                    new PluginVersion
                    {
                        SupportedProducts = new List<string>{ "2" },
                        MinimumRequiredVersionOfStudio = "16.0.1",
                        MaximumRequiredVersionOfStudio = "16.9.0",
                        VersionNumber = "4.1.1.2"
                    },
                    new VersionManifestComparison
                    {
                         IsProductMatch = false,
                         IsMinVersionMatch = false,
                         IsMaxVersionMatch = true,
                         IsVersionMatch = true,
                    },
                    false
                },
                new object[]
                {
                    new PluginVersion
                    {
                        SupportedProducts = new List<string>{ "2" },
                        MinimumRequiredVersionOfStudio = "16.0.1",
                        MaximumRequiredVersionOfStudio = "17.9.0",
                        VersionNumber = "4.1.1.2"
                    },
                    new VersionManifestComparison
                    {
                         IsProductMatch = false,
                         IsMinVersionMatch = false,
                         IsMaxVersionMatch = false,
                         IsVersionMatch = true,
                    },
                    false
                },
                new object[]
                {
                    new PluginVersion
                    {
                        SupportedProducts = new List<string>{ "2" },
                        MinimumRequiredVersionOfStudio = "16.0.1",
                        MaximumRequiredVersionOfStudio = "17.9.0",
                        VersionNumber = "4.0"
                    },
                    new VersionManifestComparison
                    {
                         IsProductMatch = false,
                         IsMinVersionMatch = false,
                         IsMaxVersionMatch = false,
                         IsVersionMatch = false,
                    },
                    false
                },
                new object[]
                {
                    new PluginVersion(),
                    new VersionManifestComparison
                    {
                         IsProductMatch = false,
                         IsMinVersionMatch = false,
                         IsMaxVersionMatch = false,
                         IsVersionMatch = false,
                    },
                    false
                },
                new object[]
                {
                    null,
                    new VersionManifestComparison
                    {
                         IsProductMatch = false,
                         IsMinVersionMatch = false,
                         IsMaxVersionMatch = false,
                         IsVersionMatch = false,
                    },
                    false
                }
            };
        }

        private static List<object[]> GetPluginMatches()
        {
            return new List<object[]>
            {
                new object[]
                {
                    new PluginDetails
                    {
                        Name = "Test",
                        Developer = new DeveloperDetails { DeveloperName = "Test Author" }
                    },
                    true, true, true
                },
                new object[]
                {
                    new PluginDetails
                    {
                        Name = "Not Test",
                        Developer = new DeveloperDetails { DeveloperName = "Test Author" }
                    },
                    false, true, false
                },
                new object[]
                {
                    new PluginDetails
                    {
                        Name = "Not Test",
                        Developer = new DeveloperDetails { DeveloperName = "Not Test Author" }
                    },
                    false, false, false
                },
                new object[]
                {
                    new PluginDetails
                    {
                        Name = "Test"
                    },
                    true, false, false
                },
                new object[] { null, false, false, false }
            };
        }
    }
}

using AppStoreIntegrationServiceManagement.ExtensionMethods;
using Xunit;

namespace AppStoreIntegrationServiceTests.AppStoreIntegrationServiceManagementTests.ExtensionMethodsTests
{
    public class StringExtentionMethodsTests
    {
        [Fact]
        public void ToUpperFirstTests_WhenFirstLetterInTheStringIsEmpty_ShouldReturnCapitalizedString()
        {
            Assert.Equal("Test", "test".ToUpperFirst());
        }
    }
}

using AppStoreIntegrationServiceCore.ExtensionMethods;
using Xunit;

namespace AppStoreIntegrationServiceTests.AppStoreIntegrationServiceManagementTests.ExtensionMethodsTests
{
    public class DictionaryExtensionMethodsTests
    {
        [Fact]
        public void ToQueryTests_WhenDictionaryIsNotEmpty_ShouldReturnTheCorrespondingQuery()
        {
            var dictionary = new Dictionary<string, string>
            {
                ["Test Key 1"] = "Test Value 1",
                ["Test Key 2"] = "Test Value 2",
                ["Test Key 3"] = "Test Value 3"
            };

            var expectedResult = "Test Key 1=Test Value 1&Test Key 2=Test Value 2&Test Key 3=Test Value 3&";
            Assert.Equal(expectedResult, dictionary.ToQuery());
        }

        [Fact]
        public void ToQueryTests_WhenDictionaryIsEmpty_ShouldReturnEmptyString()
        {
            var dictionary = new Dictionary<string, string>();

            Assert.Equal("", dictionary.ToQuery());
        }
    }
}

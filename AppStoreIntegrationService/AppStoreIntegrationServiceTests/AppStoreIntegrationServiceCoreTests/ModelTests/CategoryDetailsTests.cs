using AppStoreIntegrationServiceCore.Model;
using Xunit;

namespace AppStoreIntegrationServiceTests.AppStoreIntegrationServiceCoreTests.ModelTests
{
    public class CategoryDetailsTests
    {
        [Fact]
        public void CategoryDetailsTest_CheckIfTwoIdenticalObjectsAreEqual_ShouldReturnTrue()
        {
            Assert.True(new CategoryDetails { Id = "1", Name = "Test 1" }.Equals(new CategoryDetails { Id = "1", Name = "Test 1" }));
        }

        [Fact]
        public void CategoryDetailsTest_CheckIfTwoDifferentObjectsAreEqual_ShouldReturnFalse()
        {
            Assert.False(new CategoryDetails { Id = "2", Name = "Test 2" }.Equals(new CategoryDetails { Id = "1", Name = "Test 1" }));
        }

        [Fact]
        public void CategoryDetailsTest_CheckIfTwoObjectsAreEqualWhenOneHasNullProperties_ShouldReturnFalse()
        {
            Assert.False(new CategoryDetails { Id = null, Name = null }.Equals(new CategoryDetails { Id = "1", Name = "Test 1" }));
        }

        [Fact]
        public void CategoryDetailsTest_CheckIfTwoObjectsAreEqualWhenOneIsNull_ShouldReturnFalse()
        {
            Assert.False(new CategoryDetails { Id = "1", Name = "Test 1" }.Equals(null));
        }

        [Fact]
        public void CategoryDetailsTest_CheckIfTwoObjectsAreEqualWhenOneIsNullAndOneHasNullProperties_ShouldReturnTrue()
        {
            Assert.True(new CategoryDetails { Id = null, Name = null }.Equals(null));
        }
    }
}

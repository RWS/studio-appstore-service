﻿using AppStoreIntegrationServiceManagement.Model.Settings;
using AppStoreIntegrationServiceTests.AppStoreIntegrationServiceManagementTests.Mock;
using Xunit;

namespace AppStoreIntegrationServiceTests.AppStoreIntegrationServiceManagementTests.ModelTests.Settings
{
    public class WritableOptionsTests
    {
        [Fact]
        public void WritableOptionsTest_CheckIfTheOptionWasSaved_ShouldReturnTrue()
        {
            var writableOption = new WritableOptionsMock<SiteSettings>();
            writableOption.SaveOption(new SiteSettings { Name = "Test" });
            Assert.Equal("Test", writableOption.Value.Name);
        }
    }
}
using AppStoreIntegrationServiceCore.DataBase.Models;
using AppStoreIntegrationServiceManagement.DataBase;
using AppStoreIntegrationServiceTests.AppStoreIntegrationServiceManagementTests.Mock;
using Xunit;

namespace AppStoreIntegrationServiceTests.AppStoreIntegrationServiceManagementTests.DataBaseTests
{
    public class AccountEntitlementsManagerTests
    {
        private readonly ServiceContextFactoryMock _serviceContextFactoryMock;
        private readonly AccountEntitlementsManager _accountEntitlementsManager;

        public AccountEntitlementsManagerTests()
        {
            _serviceContextFactoryMock= new ServiceContextFactoryMock();
            _accountEntitlementsManager = new AccountEntitlementsManager(_serviceContextFactoryMock);
        }

        [Fact]
        public async Task AccountsManagerTests_PopulateDataBase_TheRecordsShouldBeAddedInTheDataBase()
        {
            await _accountEntitlementsManager.TryAddEntitlement(new AccountEntitlement { Id = "1", AccountId = "1" });
            await _accountEntitlementsManager.TryAddEntitlement(new AccountEntitlement { Id = "2", AccountId = "2" });
            await _accountEntitlementsManager.TryAddEntitlement(new AccountEntitlement { Id = "3", AccountId = "3" });

            var acountEntitlements = _serviceContextFactoryMock.CreateContext().AccountEntitlements.ToList();

            Assert.Equal(new[]
            {
                new AccountEntitlement { Id = "1", AccountId = "1" },
                new AccountEntitlement { Id = "2", AccountId = "2" },
                new AccountEntitlement { Id = "3", AccountId = "3" }
            }, acountEntitlements);

            _serviceContextFactoryMock.ClearInMemoryDataBase();
        }

        [Fact]
        public async Task AccountsManagerTests_AddRecordWithNullId_TheRecordShouldNotBeAdded()
        {
            await _accountEntitlementsManager.TryAddEntitlement(new AccountEntitlement { AccountId = "1" });

            Assert.Empty(_serviceContextFactoryMock.CreateContext().AccountEntitlements.ToList());

            _serviceContextFactoryMock.ClearInMemoryDataBase();
        }

        [Fact]
        public async Task AccountsManagerTests_AddDuplicate_TheRecordShouldNotBeAdded()
        {
            await _accountEntitlementsManager.TryAddEntitlement(new AccountEntitlement { Id = "1", AccountId = "1" });
            await _accountEntitlementsManager.TryAddEntitlement(new AccountEntitlement { Id = "1", AccountId = "2" });

            var acountEntitlements = _serviceContextFactoryMock.CreateContext().AccountEntitlements.ToList();
            Assert.Equal(new[]
            {
                    new AccountEntitlement { Id = "1", AccountId = "1" }
            }, acountEntitlements);

            _serviceContextFactoryMock.ClearInMemoryDataBase();
        }
    }
}

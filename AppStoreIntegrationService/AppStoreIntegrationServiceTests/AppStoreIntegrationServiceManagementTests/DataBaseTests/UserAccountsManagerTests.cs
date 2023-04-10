using AppStoreIntegrationServiceCore.DataBase;
using AppStoreIntegrationServiceCore.DataBase.Interface;
using AppStoreIntegrationServiceCore.DataBase.Models;
using AppStoreIntegrationServiceManagement.DataBase;
using AppStoreIntegrationServiceManagement.DataBase.Interface;
using AppStoreIntegrationServiceTests.AppStoreIntegrationServiceManagementTests.Mock;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Xunit;

namespace AppStoreIntegrationServiceTests.AppStoreIntegrationServiceManagementTests.DataBaseTests
{
    public class UserAccountsManagerTests
    {
        private readonly ServiceContextFactoryMock _serviceContextFactory;
        private readonly AccountEntitlementsManager _accountEntitlementsManager;
        private readonly AccountsManager _accountsManager;
        private readonly UserRolesManager _userRolesManager;
        private readonly UserProfilesManager _userProfilesManager;
        private readonly UserAccountsManager _userAccountsManager;

        public UserAccountsManagerTests()
        {
            _serviceContextFactory = new ServiceContextFactoryMock();
            _accountEntitlementsManager = new AccountEntitlementsManager(_serviceContextFactory);
            _accountsManager = new AccountsManager(_serviceContextFactory);
            _userRolesManager = new UserRolesManager(_serviceContextFactory);
            _userProfilesManager = new UserProfilesManager(_serviceContextFactory);
            _userAccountsManager = new UserAccountsManager
            (
                _accountEntitlementsManager,
                _serviceContextFactory,
                _accountsManager,
                _userRolesManager,
                _userProfilesManager
            );
        }

        [Fact]
        public async Task UserAccountsManagerTests_PopulateDataBase_TheRecordsShouldBeAddedInDataBase()
        {
            _ = await _userAccountsManager.TryAddUserToAccount(new UserAccount { Id = "1", UserProfileId = "1", AccountId = "1", UserRoleId = "1" });
            _ = await _userAccountsManager.TryAddUserToAccount(new UserAccount { Id = "2", UserProfileId = "2", AccountId = "2", UserRoleId = "2" });
            _ = await _userAccountsManager.TryAddUserToAccount(new UserAccount { Id = "3", UserProfileId = "3", AccountId = "3", UserRoleId = "3" });

            var userAccounts = _serviceContextFactory.CreateContext().UserAccounts.ToList();

            Assert.Equal(3, userAccounts.Count);
            Assert.Equal(new[]
            {
                new UserAccount { Id = "1", UserProfileId = "1", AccountId = "1", UserRoleId = "1" },
                new UserAccount { Id = "2", UserProfileId = "2", AccountId = "2", UserRoleId = "2" },
                new UserAccount { Id = "3", UserProfileId = "3", AccountId = "3", UserRoleId = "3" }
            }, userAccounts);

            _serviceContextFactory.ClearInMemoryDataBase();
        }

        [Fact]
        public async Task UserAccountsManagerTests_AddUserToTheSameAccount_TheDuplicateRecordShouldNotBeAdded()
        {
            _ = await _userAccountsManager.TryAddUserToAccount(new UserAccount { Id = "1", UserProfileId = "1", AccountId = "1", UserRoleId = "1" });
            var result = await _userAccountsManager.TryAddUserToAccount(new UserAccount { Id = "2", UserProfileId = "1", AccountId = "1", UserRoleId = "2" });

            var userAccounts = _serviceContextFactory.CreateContext().UserAccounts.ToList();

            Assert.Single(userAccounts);
            Assert.False(result.Succeeded);
            Assert.Equal(new[]
            {
                new UserAccount { Id = "1", UserProfileId = "1", AccountId = "1", UserRoleId = "1" },
            }, userAccounts);

            _serviceContextFactory.ClearInMemoryDataBase();
        }

        [Fact]
        public async Task UserAccountsManagerTests_TryAddNullRecord_TheDuplicateRecordShouldNotBeAdded()
        {
            var result = await _userAccountsManager.TryAddUserToAccount(null);

            Assert.Empty(_serviceContextFactory.CreateContext().UserAccounts.ToList());
            Assert.False(result.Succeeded);

            _serviceContextFactory.ClearInMemoryDataBase();
        }

        [Fact]
        public async Task UserAccountsManagerTests_GetAllUsersAssociatedToAnAccount_TheCorrespondingUsersShouldBeReturned()
        {
            _ = await _userAccountsManager.TryAddUserToAccount(new UserAccount { Id = "1", UserProfileId = "1", AccountId = "1", UserRoleId = "1" });
            _ = await _userAccountsManager.TryAddUserToAccount(new UserAccount { Id = "2", UserProfileId = "2", AccountId = "1", UserRoleId = "2" });
            _ = await _userAccountsManager.TryAddUserToAccount(new UserAccount { Id = "3", UserProfileId = "3", AccountId = "1", UserRoleId = "3" });

            await _userProfilesManager.AddUserProfile(new UserProfile { Id = "1", Email = "Test Email 1" });
            await _userProfilesManager.AddUserProfile(new UserProfile { Id = "2", Email = "Test Email 2" });
            await _userProfilesManager.AddUserProfile(new UserProfile { Id = "3", Email = "Test Email 3" });

            var users = _userAccountsManager.GetUsersFromAccount(new Account { Id = "1" });

            Assert.Equal(3, users.Count());
            Assert.Equal(new[]
            {
                new UserProfile { Id = "1", Email = "Test Email 1" },
                new UserProfile { Id = "2", Email = "Test Email 2" },
                new UserProfile { Id = "3", Email = "Test Email 3" }
            }, users);

            _serviceContextFactory.ClearInMemoryDataBase();
        }

        [Fact]
        public async Task UserAccountsManagerTests_RemoveAUserFromAccount_TheCorrespondingUsersShouldBeRemoved()
        {
            _ = await _userAccountsManager.TryAddUserToAccount(new UserAccount { Id = "1", UserProfileId = "1", AccountId = "1", UserRoleId = "1" });
            _ = await _userAccountsManager.TryAddUserToAccount(new UserAccount { Id = "2", UserProfileId = "2", AccountId = "1", UserRoleId = "2" });
            _ = await _userAccountsManager.TryAddUserToAccount(new UserAccount { Id = "3", UserProfileId = "3", AccountId = "1", UserRoleId = "3" });

            var result = await _userAccountsManager.RemoveUserFromAccount(new UserProfile { Id = "1" }, new Account { Id = "1" });
            var userAccounts = _serviceContextFactory.CreateContext().UserAccounts.ToList();

            Assert.True(result.Succeeded);
            Assert.Equal(new[]
            {
                new UserAccount { Id = "2", UserProfileId = "2", AccountId = "1", UserRoleId = "2" },
                new UserAccount { Id = "3", UserProfileId = "3", AccountId = "1", UserRoleId = "3" }
            }, userAccounts);

            _serviceContextFactory.ClearInMemoryDataBase();
        }

        [Fact]
        public async Task UserAccountsManagerTests_RemoveNullUserFromAccount_TheCollectionShouldBeUnchanged()
        {
            _ = await _userAccountsManager.TryAddUserToAccount(new UserAccount { Id = "1", UserProfileId = "1", AccountId = "1", UserRoleId = "1" });
            _ = await _userAccountsManager.TryAddUserToAccount(new UserAccount { Id = "2", UserProfileId = "2", AccountId = "1", UserRoleId = "2" });
            _ = await _userAccountsManager.TryAddUserToAccount(new UserAccount { Id = "3", UserProfileId = "3", AccountId = "1", UserRoleId = "3" });

            var result = await _userAccountsManager.RemoveUserFromAccount(null, new Account { Id = "1" });
            var userAccounts = _serviceContextFactory.CreateContext().UserAccounts.ToList();

            Assert.False(result.Succeeded);
            Assert.Equal(new[]
            {
                new UserAccount { Id = "1", UserProfileId = "1", AccountId = "1", UserRoleId = "1" },
                new UserAccount { Id = "2", UserProfileId = "2", AccountId = "1", UserRoleId = "2" },
                new UserAccount { Id = "3", UserProfileId = "3", AccountId = "1", UserRoleId = "3" }
            }, userAccounts);

            _serviceContextFactory.ClearInMemoryDataBase();
        }

        [Fact]
        public async Task UserAccountsManagerTests_GetUserRoleForTheCorrespondingAccount_TheCorrespondingRoleShouldBeReturned()
        {
            _ = await _userRolesManager.AddRole(new UserRole { Id = "1", Name = "Administrator" });
            _ = await _userAccountsManager.TryAddUserToAccount(new UserAccount { Id = "1", UserProfileId = "1", AccountId = "1", UserRoleId = "1" });
            _ = await _userAccountsManager.TryAddUserToAccount(new UserAccount { Id = "2", UserProfileId = "2", AccountId = "1", UserRoleId = "2" });
            _ = await _userAccountsManager.TryAddUserToAccount(new UserAccount { Id = "3", UserProfileId = "3", AccountId = "1", UserRoleId = "3" });

            var role = _userAccountsManager.GetUserRoleForAccount(new UserProfile { Id = "1" }, new Account { Id = "1" });

            Assert.Equal("Administrator", role.Name);

            _serviceContextFactory.ClearInMemoryDataBase();
        }

        [Fact]
        public async Task UserAccountsManagerTests_GetUserRoleForNullAccount_ShouldReturnNull()
        {
            _ = await _userRolesManager.AddRole(new UserRole { Id = "1", Name = "Administrator" });
            _ = await _userAccountsManager.TryAddUserToAccount(new UserAccount { Id = "1", UserProfileId = "1", AccountId = "1", UserRoleId = "1" });
            _ = await _userAccountsManager.TryAddUserToAccount(new UserAccount { Id = "2", UserProfileId = "2", AccountId = "1", UserRoleId = "2" });
            _ = await _userAccountsManager.TryAddUserToAccount(new UserAccount { Id = "3", UserProfileId = "3", AccountId = "1", UserRoleId = "3" });

            Assert.Null(_userAccountsManager.GetUserRoleForAccount(new UserProfile { Id = "1" }, null));

            _serviceContextFactory.ClearInMemoryDataBase();
        }

        [Fact]
        public async Task UserAccountsManagerTests_CheckIfUserBelongsToAccount_ShouldReturnTrue()
        {
            _ = await _userRolesManager.AddRole(new UserRole { Id = "1", Name = "Administrator" });
            _ = await _userAccountsManager.TryAddUserToAccount(new UserAccount { Id = "1", UserProfileId = "1", AccountId = "1", UserRoleId = "1" });
            _ = await _userAccountsManager.TryAddUserToAccount(new UserAccount { Id = "2", UserProfileId = "2", AccountId = "1", UserRoleId = "2" });
            _ = await _userAccountsManager.TryAddUserToAccount(new UserAccount { Id = "3", UserProfileId = "3", AccountId = "1", UserRoleId = "3" });

            Assert.True(_userAccountsManager.BelongsTo(new UserProfile { Id = "1" }, new Account { Id = "1" }));

            _serviceContextFactory.ClearInMemoryDataBase();
        }

        [Fact]
        public async Task UserAccountsManagerTests_CheckIfUserBelongsToAccount_ShouldReturnFalse()
        {
            _ = await _userRolesManager.AddRole(new UserRole { Id = "1", Name = "Administrator" });
            _ = await _userAccountsManager.TryAddUserToAccount(new UserAccount { Id = "1", UserProfileId = "1", AccountId = "1", UserRoleId = "1" });
            _ = await _userAccountsManager.TryAddUserToAccount(new UserAccount { Id = "2", UserProfileId = "2", AccountId = "1", UserRoleId = "2" });
            _ = await _userAccountsManager.TryAddUserToAccount(new UserAccount { Id = "3", UserProfileId = "3", AccountId = "1", UserRoleId = "3" });

            Assert.False(_userAccountsManager.BelongsTo(new UserProfile { Id = "1" }, new Account { Id = "3" }));

            _serviceContextFactory.ClearInMemoryDataBase();
        }

        [Fact]
        public async Task UserAccountsManagerTests_CheckIfNullUserBelongsToAccount_ShouldReturnFalse()
        {
            _ = await _userRolesManager.AddRole(new UserRole { Id = "1", Name = "Administrator" });
            _ = await _userAccountsManager.TryAddUserToAccount(new UserAccount { Id = "1", UserProfileId = "1", AccountId = "1", UserRoleId = "1" });
            _ = await _userAccountsManager.TryAddUserToAccount(new UserAccount { Id = "2", UserProfileId = "2", AccountId = "1", UserRoleId = "2" });
            _ = await _userAccountsManager.TryAddUserToAccount(new UserAccount { Id = "3", UserProfileId = "3", AccountId = "1", UserRoleId = "3" });

            Assert.False(_userAccountsManager.BelongsTo(null, new Account { Id = "3" }));

            _serviceContextFactory.ClearInMemoryDataBase();
        }

        [Fact]
        public async Task UserAccountsManagerTests_RemoveUserFromAllAssociatedAccounts_CorrespondingLinksShouldBeRemoved()
        {
            _ = await _userAccountsManager.TryAddUserToAccount(new UserAccount { Id = "1", UserProfileId = "1", AccountId = "1", UserRoleId = "1" });
            _ = await _userAccountsManager.TryAddUserToAccount(new UserAccount { Id = "2", UserProfileId = "1", AccountId = "2", UserRoleId = "2" });
            _ = await _userAccountsManager.TryAddUserToAccount(new UserAccount { Id = "3", UserProfileId = "1", AccountId = "3", UserRoleId = "3" });

            var result = await _userAccountsManager.RemoveUserFromAllAccounts(new UserProfile { Id = "1" });
            var userAccounts = _serviceContextFactory.CreateContext().UserAccounts.ToList();

            Assert.True(result.Succeeded);
            Assert.Empty(userAccounts);

            _serviceContextFactory.ClearInMemoryDataBase();
        }

        [Fact]
        public async Task UserAccountsManagerTests_RemoveNullUserFromAllAssociatedAccounts_CollectionShouldBeUnchanged()
        {
            _ = await _userAccountsManager.TryAddUserToAccount(new UserAccount { Id = "1", UserProfileId = "1", AccountId = "1", UserRoleId = "1" });
            _ = await _userAccountsManager.TryAddUserToAccount(new UserAccount { Id = "2", UserProfileId = "1", AccountId = "2", UserRoleId = "2" });
            _ = await _userAccountsManager.TryAddUserToAccount(new UserAccount { Id = "3", UserProfileId = "1", AccountId = "3", UserRoleId = "3" });

            var result = await _userAccountsManager.RemoveUserFromAllAccounts(null);
            var userAccounts = _serviceContextFactory.CreateContext().UserAccounts.ToList();

            Assert.False(result.Succeeded);
            Assert.Equal(new[]
            {
                new UserAccount { Id = "1", UserProfileId = "1", AccountId = "1", UserRoleId = "1" },
                new UserAccount { Id = "2", UserProfileId = "1", AccountId = "2", UserRoleId = "2" },
                new UserAccount { Id = "3", UserProfileId = "1", AccountId = "3", UserRoleId = "3" }
            }, userAccounts);

            _serviceContextFactory.ClearInMemoryDataBase();
        }

        [Fact]
        public async Task UserAccountsManagerTests_CheckIfUserCanBeRemoved_CollectionShouldBeUnchanged()
        {
            _ = await _userRolesManager.AddRole(new UserRole { Id = "1", Name = "Administrator" });
            _ = await _userAccountsManager.TryAddUserToAccount(new UserAccount { Id = "1", UserProfileId = "1", AccountId = "1", UserRoleId = "1" });
            _ = await _userAccountsManager.TryAddUserToAccount(new UserAccount { Id = "2", UserProfileId = "2", AccountId = "1", UserRoleId = "1" });
            _ = await _userAccountsManager.TryAddUserToAccount(new UserAccount { Id = "3", UserProfileId = "3", AccountId = "3", UserRoleId = "3" });

            Assert.True(_userAccountsManager.CanBeRemoved(new UserProfile { Id = "1" }));

            _serviceContextFactory.ClearInMemoryDataBase();
        }

        [Fact]
        public async Task UserAccountsManagerTests_CheckIfNullUserCanBeRemoved_CollectionShouldBeUnchanged()
        {
            _ = await _userRolesManager.AddRole(new UserRole { Id = "1", Name = "Administrator" });
            _ = await _userAccountsManager.TryAddUserToAccount(new UserAccount { Id = "1", UserProfileId = "1", AccountId = "1", UserRoleId = "1" });
            _ = await _userAccountsManager.TryAddUserToAccount(new UserAccount { Id = "2", UserProfileId = "2", AccountId = "1", UserRoleId = "1" });
            _ = await _userAccountsManager.TryAddUserToAccount(new UserAccount { Id = "3", UserProfileId = "3", AccountId = "3", UserRoleId = "3" });

            Assert.False(_userAccountsManager.CanBeRemoved(null));

            _serviceContextFactory.ClearInMemoryDataBase();
        }

        [Fact]
        public async Task UserAccountsManagerTests_GetUserUnsyncedAccount_ShouldReturnTheCorrespondingAccount()
        {
            _ = await _accountsManager.TryAddAccount(new Account { Id = "1" });
            _ = await _userAccountsManager.TryAddUserToAccount(new UserAccount { Id = "1", UserProfileId = "1", AccountId = "1", UserRoleId = "1" });
            _ = await _userAccountsManager.TryAddUserToAccount(new UserAccount { Id = "2", UserProfileId = "2", AccountId = "2", UserRoleId = "2" });
            _ = await _userAccountsManager.TryAddUserToAccount(new UserAccount { Id = "3", UserProfileId = "3", AccountId = "3", UserRoleId = "3" });

            var account = _userAccountsManager.GetUserUnsyncedAccount(new UserProfile { Id = "1" });
            Assert.Equal(new Account { Id = "1" }, account);

            _serviceContextFactory.ClearInMemoryDataBase();
        }

        [Fact]
        public async Task UserAccountsManagerTests_GetUserUnsyncedAccount_ShouldReturnNull()
        {
            _ = await _accountsManager.TryAddAccount(new Account { Id = "1" , Name = "Test Account"});
            _ = await _userAccountsManager.TryAddUserToAccount(new UserAccount { Id = "1", UserProfileId = "1", AccountId = "1", UserRoleId = "1" });
            _ = await _userAccountsManager.TryAddUserToAccount(new UserAccount { Id = "2", UserProfileId = "2", AccountId = "2", UserRoleId = "2" });
            _ = await _userAccountsManager.TryAddUserToAccount(new UserAccount { Id = "3", UserProfileId = "3", AccountId = "3", UserRoleId = "3" });

            var account = _userAccountsManager.GetUserUnsyncedAccount(new UserProfile { Id = "1" });
            Assert.Null(account);

            _serviceContextFactory.ClearInMemoryDataBase();
        }
    }
}

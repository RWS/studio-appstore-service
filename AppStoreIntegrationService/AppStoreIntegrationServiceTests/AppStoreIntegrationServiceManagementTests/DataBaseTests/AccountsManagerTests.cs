using AppStoreIntegrationServiceCore.DataBase.Models;
using AppStoreIntegrationServiceManagement.DataBase;
using AppStoreIntegrationServiceTests.AppStoreIntegrationServiceManagementTests.Mock;
using Xunit;

namespace AppStoreIntegrationServiceTests.AppStoreIntegrationServiceManagementTests.DataBaseTests
{
    public class AccountsManagerTests
    {
        private readonly ServiceContextFactoryMock _serviceContextFactoryMock;
        private readonly AccountsManager _accountsManager;

        public AccountsManagerTests()
        {
            _serviceContextFactoryMock = new ServiceContextFactoryMock();
            _accountsManager = new AccountsManager(_serviceContextFactoryMock);
        }

        [Fact]
        public async Task AccountsManagerTests_PopulateDataBase_TheRecordsShouldBeAddedInTheDataBase()
        {
            _ = await _accountsManager.TryAddAccount(new Account { Id = "1", Name = "Test Account 1" });
            _ = await _accountsManager.TryAddAccount(new Account { Id = "2", Name = "Test Account 2" });
            _ = await _accountsManager.TryAddAccount(new Account { Id = "3", Name = "Test Account 3" });

            var accounts = _serviceContextFactoryMock.CreateContext().Accounts.ToList();

            Assert.Equal(3, accounts.Count);
            Assert.Equal(new[]
            {
                new Account { Id = "1", Name = "Test Account 1" },
                new Account { Id = "2", Name = "Test Account 2" },
                new Account { Id = "3", Name = "Test Account 3" }
            }, accounts);

            _serviceContextFactoryMock.ClearInMemoryDataBase();
        }

        [Fact]
        public async Task AccountsManagerTests_TryAddNullAccount_TheDataBaseShouldBeEmpty()
        {
            var addedAccount = await _accountsManager.TryAddAccount(null);
            var accounts = _serviceContextFactoryMock.CreateContext().Accounts.ToList();

            Assert.Null(addedAccount);
            Assert.Empty(accounts);

            _serviceContextFactoryMock.ClearInMemoryDataBase();
        }

        [Fact]
        public async Task AccountsManagerTests_TryAddAccountWithNullId_TheDataBaseShouldBeEmpty()
        {
            var addedAccount = await _accountsManager.TryAddAccount(new Account { Name = "Test Account 1" });
            var accounts = _serviceContextFactoryMock.CreateContext().Accounts.ToList();

            Assert.Null(addedAccount);
            Assert.Empty(accounts);

            _serviceContextFactoryMock.ClearInMemoryDataBase();
        }

        [Fact]
        public async Task AccountsManagerTests_TryAddExistingAccount_ShouldReturnExistentAccount()
        {
            var account = new Account { Id = "1", Name = "Test Account 1" };

            _ = await _accountsManager.TryAddAccount(account);
            var existingAccount = await _accountsManager.TryAddAccount(account);
            var accounts = _serviceContextFactoryMock.CreateContext().Accounts.ToList();

            Assert.Equal(account, existingAccount);
            Assert.Single(accounts);
            Assert.Equal(new[] { account }, accounts);

            _serviceContextFactoryMock.ClearInMemoryDataBase();
        }

        [Fact]
        public async Task AccountsManagerTests_TryAddAccountWithExistingId_ShouldReturnExistentAccount()
        {
            _ = await _accountsManager.TryAddAccount(new Account { Id = "1", Name = "Test Account 1" });
            _ = await _accountsManager.TryAddAccount(new Account { Id = "1", Name = "Test Account 2" });

            var accounts = _serviceContextFactoryMock.CreateContext().Accounts.ToList();

            Assert.Single(accounts);
            Assert.Equal(new[] { new Account { Id = "1", Name = "Test Account 1" } }, accounts);

            _serviceContextFactoryMock.ClearInMemoryDataBase();
        }

        [Fact]
        public async Task AccountsManagerTests_UpdateAccountName_AccountNameShouldBeUpdated()
        {
            var account = new Account { Id = "1", Name = "Test Account 2" };

            _ = await _accountsManager.TryAddAccount(account);
            var updatedAccount = await _accountsManager.TryUpdateAccountName(account, "Test Account 2 updated");

            Assert.Equal(new Account { Id = "1", Name = "Test Account 2 updated" }, updatedAccount);
            Assert.Equal("Test Account 2 updated", updatedAccount.Name);

            _serviceContextFactoryMock.ClearInMemoryDataBase();
        }

        [Fact]
        public async Task AccountsManagerTests_UpdateAccountNameWhenNameIsNull_AccountNameShouldNotBeUpdated()
        {
            var account = new Account { Id = "1", Name = "Test Account 2" };

            _ = await _accountsManager.TryAddAccount(account);
            var updatedAccount = await _accountsManager.TryUpdateAccountName(account, null);

            Assert.Equal(account, updatedAccount);
            Assert.Equal("Test Account 2", updatedAccount.Name);

            _serviceContextFactoryMock.ClearInMemoryDataBase();
        }

        [Fact]
        public async Task AccountsManagerTests_GetAccountById_ShouldReturnTheCorrespondingAccount()
        {
            var account = new Account { Id = "1", Name = "Test Account 1" };

            _ = await _accountsManager.TryAddAccount(account);
            _ = await _accountsManager.TryAddAccount(new Account { Id = "2", Name = "Test Account 2" });
            _ = await _accountsManager.TryAddAccount(new Account { Id = "3", Name = "Test Account 3" });

            Assert.Equal(account, _accountsManager.GetAccountById("1"));

            _serviceContextFactoryMock.ClearInMemoryDataBase();
        }

        [Fact]
        public async Task AccountsManagerTests_GetAccountByNullId_ShouldReturnNull()
        {
            _ = await _accountsManager.TryAddAccount(new Account { Id = "1", Name = "Test Account 1" });
            _ = await _accountsManager.TryAddAccount(new Account { Id = "2", Name = "Test Account 2" });
            _ = await _accountsManager.TryAddAccount(new Account { Id = "3", Name = "Test Account 3" });

            Assert.Null(_accountsManager.GetAccountById(null));

            _serviceContextFactoryMock.ClearInMemoryDataBase();
        }

        [Fact]
        public async Task AccountsManagerTests_GetAccountByName_ShouldReturnTheCorrespondingAccount()
        {
            var account = new Account { Id = "2", Name = "Test Account 2" };

            _ = await _accountsManager.TryAddAccount(new Account { Id = "3", Name = "Test Account 3" });
            _ = await _accountsManager.TryAddAccount(account);
            _ = await _accountsManager.TryAddAccount(new Account { Id = "1", Name = "Test Account 1" });

            Assert.Equal(account, _accountsManager.GetAccountByName("Test Account 2"));

            _serviceContextFactoryMock.ClearInMemoryDataBase();
        }

        [Fact]
        public async Task AccountsManagerTests_GetAccountByNullName_ShouldReturnNull()
        {
            _ = await _accountsManager.TryAddAccount(new Account { Id = "1", Name = "Test Account 1" });
            _ = await _accountsManager.TryAddAccount(new Account { Id = "2", Name = "Test Account 2" });
            _ = await _accountsManager.TryAddAccount(new Account { Id = "3", Name = "Test Account 3" });

            Assert.Null(_accountsManager.GetAccountByName(null));

            _serviceContextFactoryMock.ClearInMemoryDataBase();
        }
    }
}

using AppStoreIntegrationServiceCore.DataBase.Models;
using AppStoreIntegrationServiceManagement.DataBase;
using AppStoreIntegrationServiceTests.AppStoreIntegrationServiceManagementTests.Mock;
using Xunit;

namespace AppStoreIntegrationServiceTests.AppStoreIntegrationServiceManagementTests.DataBaseTests
{
    public class AccountAgreementsManagerTests
    {
        private readonly ServiceContextFactoryMock _serviceContextFactoryMock;
        private readonly AccountAgreementsManager _accountAgreementsManager;

        public AccountAgreementsManagerTests()
        {
            _serviceContextFactoryMock = new ServiceContextFactoryMock();
            _accountAgreementsManager = new AccountAgreementsManager(_serviceContextFactoryMock);
        }

        [Fact]
        public async Task AccountAgreementsManagerTests_PopulateDataBase_TheRecordsShouldBeAddedInTheDataBase()
        {
            await _accountAgreementsManager.TryAddAgreement(new AccountAgreement { Id = "1", AccountId = "1", UserProfileId = "1" });
            await _accountAgreementsManager.TryAddAgreement(new AccountAgreement { Id = "2", AccountId = "2", UserProfileId = "2" });
            await _accountAgreementsManager.TryAddAgreement(new AccountAgreement { Id = "3", AccountId = "3", UserProfileId = "3" });
            var accountAgreements = _serviceContextFactoryMock.CreateContext().AccountAgreements.ToList();

            Assert.Equal(3, accountAgreements.Count);
            Assert.Equal(new[]
            {
                new AccountAgreement { Id = "1", AccountId = "1", UserProfileId = "1" },
                new AccountAgreement { Id = "2", AccountId = "2", UserProfileId = "2" },
                new AccountAgreement { Id = "3", AccountId = "3", UserProfileId = "3" }
            }, accountAgreements);

            _serviceContextFactoryMock.ClearInMemoryDataBase();
        }

        [Fact]
        public async Task AccountAgreementsManagerTests_AddRecordWithNullProperties_TheRecordsShouldNotBeAdded()
        {
            await _accountAgreementsManager.TryAddAgreement(new AccountAgreement { AccountId = "1", UserProfileId = "1" });
            await _accountAgreementsManager.TryAddAgreement(new AccountAgreement { Id = "2", UserProfileId = "2" });
            await _accountAgreementsManager.TryAddAgreement(new AccountAgreement { Id = "3", AccountId = "3" });

            Assert.Empty(_serviceContextFactoryMock.CreateContext().AccountAgreements.ToList());

            _serviceContextFactoryMock.ClearInMemoryDataBase();
        }

        [Fact]
        public async Task AccountAgreementsManagerTests_AddNullRecord_TheRecordsShouldNotBeAdded()
        {
            await _accountAgreementsManager.TryAddAgreement(null);

            Assert.Empty(_serviceContextFactoryMock.CreateContext().AccountAgreements.ToList());

            _serviceContextFactoryMock.ClearInMemoryDataBase();
        }

        [Fact]
        public async Task AccountAgreementsManagerTests_RemoveAllUserAggreementsForAnAccount_TheRecordsShouldBeRemoved()
        {
            await _accountAgreementsManager.TryAddAgreement(new AccountAgreement { Id = "1", AccountId = "1", UserProfileId = "1" });
            await _accountAgreementsManager.TryAddAgreement(new AccountAgreement { Id = "2", AccountId = "2", UserProfileId = "1" });
            await _accountAgreementsManager.TryAddAgreement(new AccountAgreement { Id = "3", AccountId = "3", UserProfileId = "1" });
            var accountAgreements = _serviceContextFactoryMock.CreateContext().AccountAgreements.ToList();

            Assert.Equal(3, accountAgreements.Count);
            Assert.Equal(new[]
            {
                new AccountAgreement { Id = "1", AccountId = "1", UserProfileId = "1" },
                new AccountAgreement { Id = "2", AccountId = "2", UserProfileId = "1" },
                new AccountAgreement { Id = "3", AccountId = "3", UserProfileId = "1" }
            }, accountAgreements);

            await _accountAgreementsManager.Remove(new UserProfile { Id = "1" }, new Account { Id = "1" });

            accountAgreements = _serviceContextFactoryMock.CreateContext().AccountAgreements.ToList();

            Assert.Equal(2, accountAgreements.Count);
            Assert.Equal(new[]
            {
                new AccountAgreement { Id = "2", AccountId = "2", UserProfileId = "1" },
                new AccountAgreement { Id = "3", AccountId = "3", UserProfileId = "1" }
            }, accountAgreements);

            _serviceContextFactoryMock.ClearInMemoryDataBase();
        }

        [Fact]
        public async Task AccountAgreementsManagerTests_RemoveAllUserAggreementsWhenAccountIsNull_TheRecordsShouldNotBeRemoved()
        {
            await _accountAgreementsManager.TryAddAgreement(new AccountAgreement { Id = "1", AccountId = "1", UserProfileId = "1" });
            await _accountAgreementsManager.TryAddAgreement(new AccountAgreement { Id = "2", AccountId = "2", UserProfileId = "1" });
            await _accountAgreementsManager.TryAddAgreement(new AccountAgreement { Id = "3", AccountId = "3", UserProfileId = "1" });
            var accountAgreements = _serviceContextFactoryMock.CreateContext().AccountAgreements.ToList();

            Assert.Equal(3, accountAgreements.Count);
            Assert.Equal(new[]
            {
                new AccountAgreement { Id = "1", AccountId = "1", UserProfileId = "1" },
                new AccountAgreement { Id = "2", AccountId = "2", UserProfileId = "1" },
                new AccountAgreement { Id = "3", AccountId = "3", UserProfileId = "1" }
            }, accountAgreements);

            await _accountAgreementsManager.Remove(new UserProfile { Id = "1" }, null);

            accountAgreements = _serviceContextFactoryMock.CreateContext().AccountAgreements.ToList();

            Assert.Equal(3, accountAgreements.Count);
            Assert.Equal(new[]
            {
                new AccountAgreement { Id = "1", AccountId = "1", UserProfileId = "1" },
                new AccountAgreement { Id = "2", AccountId = "2", UserProfileId = "1" },
                new AccountAgreement { Id = "3", AccountId = "3", UserProfileId = "1" }
            }, accountAgreements);

            _serviceContextFactoryMock.ClearInMemoryDataBase();
        }

        [Fact]
        public async Task AccountAgreementsManagerTests_CheckIfUserConsentAccountAgreement_ShouldReturnTrue()
        {
            await _accountAgreementsManager.TryAddAgreement(new AccountAgreement { Id = "1", AccountId = "1", UserProfileId = "1" });
            await _accountAgreementsManager.TryAddAgreement(new AccountAgreement { Id = "2", AccountId = "2", UserProfileId = "1" });
            await _accountAgreementsManager.TryAddAgreement(new AccountAgreement { Id = "3", AccountId = "3", UserProfileId = "1" });

            Assert.True(_accountAgreementsManager.HasAggreement(new UserProfile { Id = "1" }, new Account { Id = "1" }));

            _serviceContextFactoryMock.ClearInMemoryDataBase();
        }
    }
}

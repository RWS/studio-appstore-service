using AppStoreIntegrationServiceCore.DataBase;
using AppStoreIntegrationServiceCore.DataBase.Models;
using AppStoreIntegrationServiceTests.AppStoreIntegrationServiceManagementTests.Mock;
using Xunit;

namespace AppStoreIntegrationServiceTests.AppStoreIntegrationServiceManagementTests.DataBaseTests
{
    public class UserRoleManagerTests
    {
        private readonly ServiceContextFactoryMock _serviceContextFactoryMock;
        private readonly UserRolesManager _userRolesManager;

        public UserRoleManagerTests()
        {
            _serviceContextFactoryMock = new ServiceContextFactoryMock();
            _userRolesManager= new UserRolesManager(_serviceContextFactoryMock);
        }

        [Fact]
        public async Task UserRoleManagerTests_PopulateDataBase_TheRecordsShouldBeAddedInTheDataBase()
        {
            _ = await _userRolesManager.AddRole(new UserRole { Id = "1", Name = "Test Role 1" });
            _ = await _userRolesManager.AddRole(new UserRole { Id = "2", Name = "Test Role 2" });
            _ = await _userRolesManager.AddRole(new UserRole { Id = "3", Name = "Test Role 3" });

            Assert.Equal(new[]
            {
                new UserRole { Id = "1", Name = "Test Role 1" },
                new UserRole { Id = "2", Name = "Test Role 2" },
                new UserRole { Id = "3", Name = "Test Role 3" }
            }, _userRolesManager.Roles);

            _serviceContextFactoryMock.ClearInMemoryDataBase();
        }

        [Fact]
        public async Task UserRoleManagerTests_AddRecordWithIdNull_TheRecordShouldNotBeAdded()
        {
            var count = await _userRolesManager.AddRole(new UserRole { Name = "Test Role 1" });

            Assert.Equal(0, count);
            Assert.Empty(_userRolesManager.Roles);

            _serviceContextFactoryMock.ClearInMemoryDataBase();
        }

        [Fact]
        public async Task UserRoleManagerTests_AddNullRecord_TheRecordShouldNotBeAdded()
        {
            var count = await _userRolesManager.AddRole(null);

            Assert.Equal(0, count);
            Assert.Empty(_userRolesManager.Roles);

            _serviceContextFactoryMock.ClearInMemoryDataBase();
        }

        [Fact]
        public async Task UserRoleManagerTests_GetRoleById_ShouldReturnTheCorrespondingRole()
        {
            var userRole = new UserRole { Id = "1", Name = "Test Role 1" };

            _ = await _userRolesManager.AddRole(userRole);

            Assert.Equal(userRole, _userRolesManager.GetRoleById("1"));

            _serviceContextFactoryMock.ClearInMemoryDataBase();
        }

        [Fact]
        public async Task UserRoleManagerTests_GetRoleByNullId_ShouldReturnNull()
        {
            var userRole = new UserRole { Id = "1", Name = "Test Role 1" };

            _ = await _userRolesManager.AddRole(userRole);

            Assert.Null(_userRolesManager.GetRoleById(null));

            _serviceContextFactoryMock.ClearInMemoryDataBase();
        }

        [Fact]
        public async Task UserRoleManagerTests_GetRoleByName_ShouldReturnTheCorrespondingRole()
        {
            var userRole = new UserRole { Id = "1", Name = "Test Role 1" };

            _ = await _userRolesManager.AddRole(userRole);

            Assert.Equal(userRole, _userRolesManager.GetRoleByName("Test Role 1"));

            _serviceContextFactoryMock.ClearInMemoryDataBase();
        }

        [Fact]
        public async Task UserRoleManagerTests_GetRoleByInexistentName_ShouldReturnNull()
        {
            var userRole = new UserRole { Id = "1", Name = "Test Role 1" };

            _ = await _userRolesManager.AddRole(userRole);

            Assert.Null(_userRolesManager.GetRoleByName("Wrong"));

            _serviceContextFactoryMock.ClearInMemoryDataBase();
        }
    }
}

using AppStoreIntegrationServiceCore.DataBase;
using AppStoreIntegrationServiceCore.DataBase.Interface;
using AppStoreIntegrationServiceCore.DataBase.Models;
using Microsoft.EntityFrameworkCore;

namespace AppStoreIntegrationServiceTests.AppStoreIntegrationServiceManagementTests.Mock
{
    public class ServiceContextFactoryMock : IServiceContextFactory
    {
        public ServiceContextFactoryMock()
        {
            ClearInMemoryDataBase();
        }

        public AppStoreIntegrationServiceContext CreateContext()
        {
            var builder = new DbContextOptionsBuilder<AppStoreIntegrationServiceContext>();
            var options = builder.UseInMemoryDatabase(databaseName: "TestDatabase").Options;

            var context = new AppStoreIntegrationServiceContext(options);
            return context;
        }

        public void ClearInMemoryDataBase()
        {
            var builder = new DbContextOptionsBuilder<AppStoreIntegrationServiceContext>();
            var options = builder.UseInMemoryDatabase(databaseName: "TestDatabase").Options;

            var context = new AppStoreIntegrationServiceContext(options);

            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }
    }
}

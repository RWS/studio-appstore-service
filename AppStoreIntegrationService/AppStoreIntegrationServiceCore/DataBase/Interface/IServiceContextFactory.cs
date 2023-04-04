namespace AppStoreIntegrationServiceCore.DataBase.Interface
{
    public interface IServiceContextFactory
    {
        AppStoreIntegrationServiceContext CreateContext();
    }
}

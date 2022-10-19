namespace AppStoreIntegrationServiceCore.Repository.V2.Interface
{
    public interface IVersionProvider
    {
        Task<string> GetAPIVersion();
    }
}

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface IVersionProvider
    {
        Task<string> GetAPIVersion();
        Task UpdateAPIVersion(string version);
    }
}

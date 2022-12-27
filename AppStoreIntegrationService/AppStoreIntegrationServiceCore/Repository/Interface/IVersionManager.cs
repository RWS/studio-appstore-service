namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface IVersionManager
    {
        Task SaveVersion(string version);
        Task<string> GetVersion();
    }
}

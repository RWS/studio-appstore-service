namespace AppStoreIntegrationServiceCore.Repository.V1.Interface
{
    public interface ILocalRepository<T>
    {
        Task<List<T>> ReadPluginsFromFile();
    }
}

using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceAPI.Model.Repository.Interface
{
    public interface IResponseRepository<T>
    {
        Task<PluginResponse<T>> GetResponse();
    }
}

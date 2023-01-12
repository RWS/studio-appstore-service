using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface IResponseManager
    {
        Task<PluginResponse<PluginDetails>> GetResponse();
    }
}

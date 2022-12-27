using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface IResponseManager
    {
        Task<PluginResponse<PluginDetails<PluginVersion<string>, string>>> GetResponse();
    }
}

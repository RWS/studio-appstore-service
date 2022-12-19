using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceAPI.Model.Repository.Interface
{
    public interface IResponseRepository
    {
        Task<PluginResponse<PluginDetails<PluginVersion<string>, string>>> GetResponse();
    }
}

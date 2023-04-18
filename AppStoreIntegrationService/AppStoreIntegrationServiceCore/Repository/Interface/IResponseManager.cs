using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface IResponseManager
    {
        Task<PluginResponse<PluginDetails>> GetResponse();
        Task SaveResponse(PluginResponse<PluginDetails> response);
        Task<PluginResponseBase<PluginDetails>> GetBaseResponse();
    }
}
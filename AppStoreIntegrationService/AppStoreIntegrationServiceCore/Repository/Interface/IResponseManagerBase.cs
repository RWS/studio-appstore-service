using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface IResponseManagerBase
    {
        Task<PluginResponseBase<PluginDetails>> GetBaseResponse();
    }
}

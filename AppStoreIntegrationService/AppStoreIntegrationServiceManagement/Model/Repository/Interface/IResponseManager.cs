using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;

namespace AppStoreIntegrationServiceManagement.Model.Repository.Interface;

public interface IResponseManager : IResponseManagerBase
{
    Task<PluginResponse<PluginDetails>> GetResponse();
    Task SaveResponse(PluginResponse<PluginDetails> response);
}

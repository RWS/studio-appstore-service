using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceAPI.Model.Repository.Interface
{
    public interface IPluginResponseConverter<T, U>
    {
        PluginResponse<U> CreateOldResponse(PluginResponse<T> newResponse);
    }
}

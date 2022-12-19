using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceAPI.Model.Repository.Interface
{
    public interface IPluginResponseConverter
    {
        PluginResponse<PluginDetails<PluginVersion<ProductDetails>, CategoryDetails>> CreateOldResponse(PluginResponse<PluginDetails<PluginVersion<string>, string>> newResponse);
    }
}

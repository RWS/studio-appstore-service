using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceAPI.Model.Repository.Interface
{
    public interface IPluginResponseConverter
    {
        PluginResponse<PluginDetailsBase<PluginVersionBase<ProductDetails>, CategoryDetails>> CreateOldResponse(PluginResponse<PluginDetails> newResponse);
        PluginResponse<PluginDetailsBase<PluginVersionBase<string>, string>> CreateBaseResponse(PluginResponse<PluginDetails> newResponse);
    }
}

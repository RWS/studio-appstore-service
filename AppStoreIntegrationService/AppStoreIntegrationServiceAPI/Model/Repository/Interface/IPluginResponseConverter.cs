using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceAPI.Model.Repository.Interface
{
    public interface IPluginResponseConverter
    {
        List<PluginDetailsBase<PluginVersionBase<ProductDetails>, CategoryDetails>> CreateOldResponse(PluginResponseBase<PluginDetails> newResponse);
        PluginResponseBase<PluginDetailsBase<PluginVersionBase<string>, string>> CreateBaseResponse(PluginResponseBase<PluginDetails> newResponse);
    }
}

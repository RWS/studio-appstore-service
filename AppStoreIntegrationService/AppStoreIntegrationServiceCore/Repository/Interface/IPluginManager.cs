using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface IPluginManager
    {
        Task<IEnumerable<PluginDetails>> ReadPlugins();
        Task SavePlugins(IEnumerable<PluginDetails> plugins);
        Task<IEnumerable<PluginDetails>> ReadPending();
        Task SavePending(IEnumerable<PluginDetails> plugins);
        Task<IEnumerable<PluginDetails>> ReadDrafts();
        Task SaveDrafts(IEnumerable<PluginDetails> plugins);
    }
}

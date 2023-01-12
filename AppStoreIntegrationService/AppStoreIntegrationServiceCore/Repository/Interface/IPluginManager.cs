using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface IPluginManager
    {
        Task<IEnumerable<PluginDetails>> ReadPlugins();
        Task SavePlugins(IEnumerable<PluginDetails> plugins);
        Task BackupPlugins(IEnumerable<PluginDetails> plugins);
    }
}

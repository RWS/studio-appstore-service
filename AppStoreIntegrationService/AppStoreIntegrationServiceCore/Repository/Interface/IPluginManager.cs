using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface IPluginManager
    {
        Task<IEnumerable<PluginDetails<PluginVersion<string>, string>>> ReadPlugins();
        Task SavePlugins(IEnumerable<PluginDetails<PluginVersion<string>, string>> plugins);
        Task BackupPlugins(IEnumerable<PluginDetails<PluginVersion<string>, string>> plugins);
    }
}

using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface IPluginManager
    {
        Task<List<PluginDetails<PluginVersion<string>, string>>> GetPlugins();
        Task SavePlugins(List<PluginDetails<PluginVersion<string>, string>> plugins);
        Task BackupPlugins(List<PluginDetails<PluginVersion<string>, string>> plugins);
    }
}

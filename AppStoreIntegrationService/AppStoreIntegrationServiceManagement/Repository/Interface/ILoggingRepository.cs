using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceManagement.Repository.Interface
{
    public interface ILoggingRepository
    {
        Task Log(string username, int pluginId, string custom);
        Task ClearLogs(int pluginId);
        IEnumerable<Log> SearchLogs(IEnumerable<Log> logs, DateTime from, DateTime to, string query = null);
        Task<IEnumerable<Log>> GetPluginLogs(int pluginId);
        string CreateChangesLog(PluginDetails latest, PluginDetails old, string username);
        string CreateChangesLog(PluginVersion @new, PluginVersion old, string username);
    }
}

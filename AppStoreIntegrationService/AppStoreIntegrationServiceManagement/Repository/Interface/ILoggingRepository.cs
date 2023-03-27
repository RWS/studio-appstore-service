using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceManagement.Repository.Interface
{
    public interface ILoggingRepository
    {
        Task Log(Log log, int pluginId);
        Task ClearLogs(int pluginId);
        IEnumerable<Log> SearchLogs(IEnumerable<Log> logs, DateTime from, DateTime to, string query = null);
        Task<IEnumerable<Log>> GetPluginLogs(int pluginId);
    }
}

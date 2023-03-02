using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceManagement.Model.Repository.Interface
{
    public interface ILoggingRepository
    {
        Task Log(string username, int pluginId, string custom);
        Task ClearLogs(int pluginId);
        IEnumerable<Log> SearchLogs(IEnumerable<Log> logs, DateTime from, DateTime to, string query = null);
        Task<IEnumerable<Log>> GetPluginLogs(int pluginId);
    }
}

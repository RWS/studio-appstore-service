using AppStoreIntegrationServiceCore.Model;
using System.Security.Claims;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface ILoggingRepository
    {
        Task Log(string username, int pluginId, string custom);
        IEnumerable<Log> SearchLogs(IEnumerable<Log> logs, DateTime from, DateTime to, string query = null);
        Task<IEnumerable<Log>> GetPluginLogs(int pluginId);
    }
}

using AppStoreIntegrationServiceCore.Model;
using System.Security.Claims;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface ILoggingRepository
    {
        Task Log(ClaimsPrincipal user, PluginDetailsBase<PluginVersionBase<string>, string> current, PluginDetailsBase<PluginVersionBase<string>, string> old = null);
        Task Log(ClaimsPrincipal user, int pluginId, PluginVersionBase<string> current, PluginVersionBase<string> old = null);
        Task Log(ClaimsPrincipal user, int pluginId, string custom);
        IEnumerable<Log> SearchLogs(IEnumerable<Log> logs, DateTime from, DateTime to, string query = null);
        Task<IEnumerable<Log>> GetPluginLogs(int pluginId);
    }
}

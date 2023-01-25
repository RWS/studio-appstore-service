using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace AppStoreIntegrationServiceCore.Repository
{
    public class LoggingRepository : ILoggingRepository
    {
        private readonly ILogsManager _logsManager;

        public LoggingRepository(ILogsManager logsManager)
        {
            _logsManager = logsManager;
        }

        public async Task<IEnumerable<Log>> GetPluginLogs(int pluginId)
        {
            var logs = await _logsManager.ReadLogs();
            return logs.TryGetValue(pluginId, out var pluginLogs) ? pluginLogs : Enumerable.Empty<Log>();
        }

        public async Task Log(string username, int pluginId, string custom)
        {
            await Save(new Log
            {
                Author = username,
                Date = DateTime.Now,
                Description = custom
            }, pluginId);
        }

        private async Task Save(Log log, int pluginId)
        {
            var logs = await _logsManager.ReadLogs();

            if (logs.TryGetValue(pluginId, out var pluginLogs))
            {
                logs[pluginId] = pluginLogs.Append(log);
            }
            else
            {
                logs.Add(pluginId, new List<Log> { log });
            }

            await _logsManager.UpdateLogs(logs);
        }

        public IEnumerable<Log> SearchLogs(IEnumerable<Log> logs, DateTime from, DateTime to, string query = null)
        {
            var filtered = logs.Where(x => x.Date >= from && x.Date <= to);
            if (string.IsNullOrEmpty(query))
            {
                return filtered;
            }

            return filtered.Where(x => Regex.IsMatch(x.Description, query, RegexOptions.IgnoreCase));
        }
    }
}

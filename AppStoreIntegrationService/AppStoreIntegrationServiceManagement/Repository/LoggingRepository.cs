using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using AppStoreIntegrationServiceManagement.Repository.Interface;
using System.Text.RegularExpressions;

namespace AppStoreIntegrationServiceManagement.Repository
{
    public class LoggingRepository : ILoggingRepository
    {
        private readonly IResponseManager _responseManager;

        public LoggingRepository(IResponseManager responseManager)
        {
            _responseManager = responseManager;
        }

        public async Task<IEnumerable<Log>> GetPluginLogs(int pluginId)
        {
            var logs = await GetAllLogs();
            return logs.TryGetValue(pluginId, out var pluginLogs) ? pluginLogs : Enumerable.Empty<Log>();
        }

        public async Task Log(Log log, int pluginId)
        {
            if (log == null || !log.HasChanges())
            {
                return;
            }

            var logs = await GetAllLogs();

            if (logs.TryGetValue(pluginId, out var pluginLogs))
            {
                logs[pluginId] = pluginLogs.Append(log);
            }
            else
            {
                logs.Add(pluginId, new List<Log> { log });
            }

            await UpdateLogs(logs);
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

        public async Task ClearLogs(int pluginId)
        {
            var logs = await GetAllLogs();

            if (logs.ContainsKey(pluginId))
            {
                logs[pluginId] = new List<Log>();
            }

            await UpdateLogs(logs);
        }

        private async Task<IDictionary<int, IEnumerable<Log>>> GetAllLogs()
        {
            var data = await _responseManager.GetResponse();
            return data.Logs;
        }

        private async Task UpdateLogs(IDictionary<int, IEnumerable<Log>> logs)
        {
            var data = await _responseManager.GetResponse();
            data.Logs = logs;
            await _responseManager.SaveResponse(data);
        }
    }
}

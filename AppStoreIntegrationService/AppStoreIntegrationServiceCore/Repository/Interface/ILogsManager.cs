using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface ILogsManager
    {
        Task<IDictionary<int, IEnumerable<Log>>> ReadLogs();
        Task UpdateLogs(IDictionary<int, IEnumerable<Log>> package);
    }
}

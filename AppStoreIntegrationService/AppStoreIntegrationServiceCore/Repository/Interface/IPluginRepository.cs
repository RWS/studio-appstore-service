using AppStoreIntegrationServiceCore.Model;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface IPluginRepository
    {
        Task RemovePlugin(int id);
        Task<bool> ExitsPlugin(int id);
        Task<bool> HasActiveChanges(int id);
        Task<bool> HasDraftChanges(int id, string userRole = null);
        Task<bool> HasPendingChanges(int id, string userRole = null);
        Task SavePlugin(PluginDetails plugin, bool removeOtherVersions = false);
        Task<PluginDetails> GetPluginById(int id, string username = null, string userRole = null, Status status = Status.All);
        Task<IEnumerable<PluginDetails>> GetAll(string sortOrder, string username = null, string userRole = null, Status status = Status.All);
    }
}

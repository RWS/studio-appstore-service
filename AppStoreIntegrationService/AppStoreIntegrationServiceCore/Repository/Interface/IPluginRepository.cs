using AppStoreIntegrationServiceCore.Model;
using System.Security.Claims;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface IPluginRepository
    {
        Task RemovePlugin(int id);
        Task<bool> ExitsPlugin(int id);
        Task<bool> HasActiveChanges(int id);
        Task<bool> HasDraftChanges(int id, ClaimsPrincipal user = null);
        Task<bool> HasPendingChanges(int id, ClaimsPrincipal user = null);
        Task SavePlugin(PluginDetails plugin, bool removeOtherVersions = false);
        Task<PluginDetails> GetPluginById(int id, Status status = Status.All, ClaimsPrincipal user = null);
        Task<IEnumerable<PluginDetails>> GetAll(string sortOrder, ClaimsPrincipal user = null, Status status = Status.All);
    }
}

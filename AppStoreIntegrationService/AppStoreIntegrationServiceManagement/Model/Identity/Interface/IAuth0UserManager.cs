using AppStoreIntegrationServiceCore.DataBase.Models;
using AppStoreIntegrationServiceManagement.Helpers;

namespace AppStoreIntegrationServiceManagement.Model.Identity.Interface
{
    public interface IAuth0UserManager
    {
        Task<IEnumerable<UserProfile>> GetUsers();
        Task<UserProfile> GetUserById(string id);
        Task<Auth0Response> TryCreateUser(RegisterModel model);
        Task<Auth0Response> TryUpdateUserEmail(string userId, string email);
    }
}

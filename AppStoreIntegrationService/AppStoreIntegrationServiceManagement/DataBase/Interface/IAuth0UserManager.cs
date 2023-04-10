using AppStoreIntegrationServiceCore.DataBase.Models;
using AppStoreIntegrationServiceManagement.DataBase.Models;
using AppStoreIntegrationServiceManagement.Model.Identity;

namespace AppStoreIntegrationServiceManagement.DataBase.Interface
{
    public interface IAuth0UserManager
    {
        Task<UserProfile> GetUserByEmail(string email);
        Task<Auth0Response> TryCreateUser(RegisterModel model);
    }
}

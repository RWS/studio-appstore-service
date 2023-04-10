using AppStoreIntegrationServiceCore.DataBase.Models;
using System.Security.Claims;

namespace AppStoreIntegrationServiceCore.DataBase.Interface
{
    public interface IUserProfilesManager
    {
        List<UserProfile> UserProfiles { get; }
        Task AddUserProfile(UserProfile profile);
        Task UpdateUserProfile(UserProfile profile);
        Task UpdateUserName(UserProfile profile, string name);
        Task UpdateUserId(UserProfile profile, string userId);
        UserProfile GetUser(ClaimsPrincipal principal);
        UserProfile GetUserByEmail(string email);
        UserProfile GetUserById(string id);
        Task Delete(UserProfile profile);
    }
}

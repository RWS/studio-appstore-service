using AppStoreIntegrationServiceCore.DataBase.Models;
using System.Security.Claims;

namespace AppStoreIntegrationServiceCore.DataBase.Interface
{
    public interface IUserProfilesManager
    {
        List<UserProfile> UserProfiles { get; }
        void AddUserProfile(UserProfile profile);
        void UpdateUserProfile(UserProfile profile);
        void UpdateUserName(UserProfile profile, string name);
        void UpdateUserId(UserProfile profile, string userId);
        UserProfile GetUser(ClaimsPrincipal principal);
        UserProfile GetUserByName(string username);
        UserProfile GetUserByEmail(string email);
        UserProfile GetUserById(string id);
        void UpdateUserEmail(UserProfile user, string email);
        void Delete(UserProfile profile);
    }
}

using AppStoreIntegrationServiceCore.DataBase.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace AppStoreIntegrationServiceCore.DataBase.Interface
{
    public interface IUserProfilesManager
    {
        List<UserProfile> UserProfiles { get; }
        Task<IdentityResult> TryAddUserProfile(UserProfile profile);
        Task<IdentityResult> TryUpdateUserProfile(UserProfile profile);
        UserProfile GetUser(ClaimsPrincipal principal);
        UserProfile GetUserByEmail(string email);
        UserProfile GetUserById(string id);
    }
}
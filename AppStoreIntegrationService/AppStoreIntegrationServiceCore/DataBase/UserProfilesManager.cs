using AppStoreIntegrationServiceCore.DataBase.Interface;
using AppStoreIntegrationServiceCore.DataBase.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace AppStoreIntegrationServiceCore.DataBase
{
    public class UserProfilesManager : IUserProfilesManager
    {
        private readonly IServiceContextFactory _serviceContext;

        public UserProfilesManager(IServiceContextFactory serviceContext)
        {
            _serviceContext = serviceContext;
        }

        public List<UserProfile> UserProfiles
        {
            get
            {
                using (var context = _serviceContext.CreateContext())
                {
                    return context.UserProfiles.ToList();
                }
            }
        }

        public async Task<IdentityResult> TryAddUserProfile(UserProfile profile)
        {
            try
            {
                using (var context = _serviceContext.CreateContext())
                {
                    context.UserProfiles.Add(profile);
                    await context.SaveChangesAsync();
                }

                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = ex.Message });
            }
        }

        public async Task<IdentityResult> TryUpdateUserProfile(UserProfile profile)
        {
            try
            {
                using (var context = _serviceContext.CreateContext())
                {
                    var user = context.UserProfiles.FirstOrDefault(x => x.Id == profile.Id);

                    user.Id = profile.Id;
                    user.UserId = profile.UserId;
                    user.Email = profile.Email;
                    user.Name = profile.Name;
                    user.Picture = profile.Picture;
                    user.EmailNotificationsEnabled = profile.EmailNotificationsEnabled;
                    user.PushNotificationsEnabled = profile.PushNotificationsEnabled;
                    user.RememberAccount = profile.RememberAccount;
                    user.SelectedAccountId = profile.SelectedAccountId;
                    user.APIAccessToken = profile.APIAccessToken;

                    await context.SaveChangesAsync();
                }

                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = ex.Message });
            }
        }

        public UserProfile GetUser(ClaimsPrincipal principal)
        {
            var email = principal?.Claims?.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            if (email == null)
            {
                return null;
            }

            using (var context = _serviceContext.CreateContext())
            {
                return context.UserProfiles.ToList().FirstOrDefault(x => x.Email == email);
            }
        }

        public UserProfile GetUserByEmail(string email)
        {
            using (var context = _serviceContext.CreateContext())
            {
                return context.UserProfiles.ToList().FirstOrDefault(x => x.Email == email);
            }
        }

        public UserProfile GetUserById(string id)
        {
            using (var context = _serviceContext.CreateContext())
            {
                return context.UserProfiles.ToList().FirstOrDefault(x => x.Id == id);
            }
        }

        public static string GetUserId(ClaimsPrincipal principal)
        {
            return principal.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}
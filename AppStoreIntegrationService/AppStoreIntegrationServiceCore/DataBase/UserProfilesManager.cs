using AppStoreIntegrationServiceCore.DataBase.Interface;
using AppStoreIntegrationServiceCore.DataBase.Models;
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
                using var context = _serviceContext.CreateContext();
                return context.UserProfiles.ToList();
            }
        }

        public void AddUserProfile(UserProfile profile)
        {
            using var context = _serviceContext.CreateContext();
            context.UserProfiles.Add(profile);
            context.SaveChanges();
        }

        public void UpdateUserProfile(UserProfile profile)
        {
            using var context = _serviceContext.CreateContext();
            var user = context.UserProfiles.FirstOrDefault(x => x.Id == profile.Id);
            context.Remove(user);
            context.SaveChanges();
            context.Add(profile);
            context.SaveChanges();
        }

        public void UpdateUserName(UserProfile profile, string name)
        {
            using var context = _serviceContext.CreateContext();
            var user = context.UserProfiles.FirstOrDefault(x => x.Id == profile.Id);
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            user.Name = name;
            context.SaveChanges();
        }

        public void UpdateUserId(UserProfile profile, string userId)
        {
            using var context = _serviceContext.CreateContext();
            var user = context.UserProfiles.FirstOrDefault(x => x.Id == profile.Id);
            if (string.IsNullOrEmpty(userId))
            {
                return;
            }

            user.UserId = userId;
            context.SaveChanges();
        }

        public UserProfile GetUser(ClaimsPrincipal principal)
        {
            var email = principal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            if (email == null)
            {
                return null;
            }

            using var context = _serviceContext.CreateContext();
            return context.UserProfiles.ToList().FirstOrDefault(x => x.Email == email);
        }

        public UserProfile GetUserByName(string username)
        {
            using var context = _serviceContext.CreateContext();
            return context.UserProfiles.ToList().FirstOrDefault(x => x.Name == username);
        }

        public UserProfile GetUserByEmail(string email)
        {
            using var context = _serviceContext.CreateContext();
            return context.UserProfiles.ToList().FirstOrDefault(x => x.Email == email);
        }

        public UserProfile GetUserById(string id)
        {
            using var context = _serviceContext.CreateContext();
            return context.UserProfiles.ToList().FirstOrDefault(x => x.Id == id);
        }

        public static string GetUserId(ClaimsPrincipal principal)
        {
            return principal.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        public void UpdateUserEmail(UserProfile user, string email)
        {
            using var context = _serviceContext.CreateContext();
            var oldUser = context.UserProfiles.Find(user.Id);

            if (string.IsNullOrEmpty(email))
            {
                return;
            }

            oldUser.Email = email;
            context.SaveChanges();
        }

        public void Delete(UserProfile profile)
        {
            using var context = _serviceContext.CreateContext();
            var oldProfile = context.UserProfiles.FirstOrDefault(x => x.Id == profile.Id);
            context.UserProfiles.Remove(oldProfile);
            context.SaveChanges();
        }
    }
}

using AppStoreIntegrationServiceCore.DataBase.Interface;
using AppStoreIntegrationServiceCore.DataBase.Models;
using AppStoreIntegrationServiceManagement.DataBase.Interface;
using AppStoreIntegrationServiceManagement.ExtensionMethods;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace AppStoreIntegrationServiceManagement.Filters
{
    public class DBSynchedFilter : IAsyncAuthorizationFilter
    {
        private readonly IUserProfilesManager _userProfilesManager;
        private readonly IUserAccountsManager _userAccountsManager;
        private readonly IAccountsManager _accountsManager;

        public DBSynchedFilter
        (
            IUserProfilesManager userProfilesManager, 
            IUserAccountsManager userAccountsManager,
            IAccountsManager accountsManager
        )
        {
            _userProfilesManager = userProfilesManager;
            _userAccountsManager = userAccountsManager;
            _accountsManager = accountsManager;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var principal = context.HttpContext.User;
            var user = _userProfilesManager.GetUser(principal);
            var username = principal.Claims.FirstOrDefault(x => x.Type == "Username")?.Value;

            if (user == null)
            {
                var returnUrl = context.HttpContext.Request.Path.Value;
                context.HttpContext.Response.Redirect($"~/AccessDenied?{returnUrl}");
                return;
            }

            await SyncUserProfile(user, principal, username);
            await SyncUserAccount(user, username);
        }

        private async Task SyncUserAccount(UserProfile user, string username)
        {
            var account = _userAccountsManager.GetUserUnsyncedAccount(user);

            if (account == null)
            {
                return;
            }

            await _accountsManager.TryUpdateAccountName(account, $"{username.ToUpperFirst()} Account");
        }

        private async Task SyncUserProfile(UserProfile user, ClaimsPrincipal principal, string username)
        {
            if (!string.IsNullOrEmpty(user.UserId) && !string.IsNullOrEmpty(user.Name))
            {
                return;
            }

            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            await _userProfilesManager.UpdateUserName(user, username);
            await _userProfilesManager.UpdateUserId(user, userId);
        }
    }
}

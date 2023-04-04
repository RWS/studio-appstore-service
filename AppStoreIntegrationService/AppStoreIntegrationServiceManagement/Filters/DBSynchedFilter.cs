using AppStoreIntegrationServiceCore.DataBase.Interface;
using AppStoreIntegrationServiceCore.DataBase.Models;
using AppStoreIntegrationServiceManagement.ExtensionMethods;
using AppStoreIntegrationServiceManagement.Model.Identity.Interface;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace AppStoreIntegrationServiceManagement.Filters
{
    public class DBSynchedFilter : IAuthorizationFilter
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

        public void OnAuthorization(AuthorizationFilterContext context)
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

            SyncUserProfile(user, principal, username);
            SyncUserAccount(user, username);
        }

        private void SyncUserAccount(UserProfile user, string username)
        {
            var account = _userAccountsManager.GetUserUnsyncedAccount(user);

            if (account == null)
            {
                return;
            }

            _accountsManager.UpdateAccountName(account, $"{username.ToUpperFirst()} Account");
        }

        private void SyncUserProfile(UserProfile user, ClaimsPrincipal principal, string username)
        {
            if (!string.IsNullOrEmpty(user.UserId) && !string.IsNullOrEmpty(user.Name))
            {
                return;
            }

            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            _userProfilesManager.UpdateUserName(user, username);
            _userProfilesManager.UpdateUserId(user, userId);
        }
    }
}

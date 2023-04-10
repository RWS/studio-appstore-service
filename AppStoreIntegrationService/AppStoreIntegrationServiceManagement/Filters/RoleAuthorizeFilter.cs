using AppStoreIntegrationServiceCore.DataBase.Interface;
using AppStoreIntegrationServiceManagement.DataBase.Interface;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AppStoreIntegrationServiceManagement.Filters
{
    public class RoleAuthorizeFilter : IAuthorizationFilter
    {
        private readonly IUserProfilesManager _userProfilesManager;
        private readonly IUserAccountsManager _userAccountsManager;
        private readonly IUserRolesManager _userRolesManager;
        private readonly IAccountsManager _accountsManager;
        private readonly string[] _roles;

        public RoleAuthorizeFilter
        (
            IUserProfilesManager userManager,
            IUserAccountsManager userAccountsManager,
            IUserRolesManager roleManager,
            IAccountsManager accountsManager,
            string[] roles
        )
        {
            _userProfilesManager = userManager;
            _userAccountsManager = userAccountsManager;
            _userRolesManager = roleManager;
            _accountsManager = accountsManager;
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = _userProfilesManager.GetUser(context.HttpContext.User);
            var account = _accountsManager.GetAccountById(user.SelectedAccountId);
            var userRole = _userAccountsManager.GetUserRoleForAccount(user, account);

            if (_roles.Any(x => _userRolesManager.GetRoleByName(x).Equals(userRole)))
            {
                return;
            }

            var redirectUrl = context.HttpContext.Request.Path.Value;
            context.HttpContext.Response.Redirect($"~/AccessDenied?{redirectUrl}");
        }
    }
}

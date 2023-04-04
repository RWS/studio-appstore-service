using AppStoreIntegrationServiceCore.DataBase.Interface;
using AppStoreIntegrationServiceCore.DataBase.Models;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AppStoreIntegrationServiceManagement.Filters
{
    public class RoleAuthorizeFilter : IAuthorizationFilter
    {
        private readonly IUserProfilesManager _userManager;
        private readonly IUserAccountsManager _userAccountsManager;
        private readonly IUserRolesManager _roleManager;
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
            _userManager = userManager;
            _userAccountsManager = userAccountsManager;
            _roleManager = roleManager;
            _accountsManager = accountsManager;
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = _userManager.GetUser(context.HttpContext.User);
            var account = _accountsManager.GetAccountById(user.SelectedAccountId);
            var userRole = _userAccountsManager.GetUserRoleForAccount(user, account);

            if (_roles.Any(x => _roleManager.GetRoleByName(x).Equals(userRole)))
            {
                return;
            }

            var redirectUrl = context.HttpContext.Request.Path.Value;
            context.HttpContext.Response.Redirect($"~/AccessDenied?{redirectUrl}");
        }
    }
}

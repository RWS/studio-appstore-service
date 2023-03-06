using AppStoreIntegrationServiceManagement.Model.DataBase;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AppStoreIntegrationServiceManagement.Filters
{
    public class RoleAuthorizeFilter : IAsyncAuthorizationFilter
    {
        private readonly UserManager<IdentityUserExtended> _userManager;
        private readonly UserAccountsManager _userAccountsManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly string[] _roles;

        public RoleAuthorizeFilter
        (
            UserManager<IdentityUserExtended> userManager,
            UserAccountsManager userAccountsManager,
            RoleManager<IdentityRole> roleManager,
            string[] roles
        )
        {
            _userManager = userManager;
            _userAccountsManager = userAccountsManager;
            _roleManager = roleManager;
            _roles = roles;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = await _userManager.GetUserAsync(context.HttpContext.User);
            foreach (var role in _roles)
            {
                var identityRole = await _roleManager.FindByNameAsync(role);
                if (_userAccountsManager.IsInRole(user, identityRole.Id))
                {
                    return;
                }
            }

            var redirectUrl = context.HttpContext.Request.Path.Value;
            context.HttpContext.Response.Redirect($"~/AccountDenied?{redirectUrl}");
        }
    }
}

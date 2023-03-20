using AppStoreIntegrationServiceCore.DataBase;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AppStoreIntegrationServiceManagement.Filters
{
    public class OwnerFilter : IAsyncAuthorizationFilter
    {
        private readonly UserManager<IdentityUserExtended> _userManager;
        private readonly UserAccountsManager _userAccountsManager;

        public OwnerFilter
        (
            UserManager<IdentityUserExtended> userManager, 
            UserAccountsManager userAccountsManager
        )
        {
            _userManager = userManager;
            _userAccountsManager = userAccountsManager;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = await _userManager.GetUserAsync(context.HttpContext.User);

            if (_userAccountsManager.IsOwner(user))
            {
                return;
            }

            var redirectUrl = context.HttpContext.Request.Path.Value;
            context.HttpContext.Response.Redirect($"~/AccountDenied?{redirectUrl}");
        }
    }
}

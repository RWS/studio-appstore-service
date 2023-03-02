using AppStoreIntegrationServiceManagement.Model.DataBase;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AppStoreIntegrationServiceManagement.Filters
{
    public class AccountSelectedFilter : IAsyncAuthorizationFilter
    {
        const string Url = "/Identity/Authentication/Accounts?ReturnUrl={0}";
        private readonly UserManager<IdentityUserExtended> _userManager;
        private readonly UserAccountsManager _userAccountsManager;

        public AccountSelectedFilter
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
            var accounts = _userAccountsManager.GetUserAccounts(user);

            if (accounts.Count() <= 1)
            {
                user.SelectedAccountId = accounts.FirstOrDefault().Id;
                await _userManager.UpdateAsync(user);
                return;
            }

            if (!string.IsNullOrEmpty(user.SelectedAccountId))
            {
                return;
            }

            var returnUrl = context.HttpContext.Request.Path.Value;
            context.Result = new RedirectResult(string.Format(Url, returnUrl));
            context.HttpContext.Response.Redirect(string.Format(Url, returnUrl));
        }
    }
}

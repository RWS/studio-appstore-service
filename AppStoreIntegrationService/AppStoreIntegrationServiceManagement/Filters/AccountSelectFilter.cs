using AppStoreIntegrationServiceCore.DataBase.Interface;
using AppStoreIntegrationServiceManagement.DataBase.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AppStoreIntegrationServiceManagement.Filters
{
    public class AccountSelectFilter : IAsyncAuthorizationFilter
    {
        const string Url = "/Identity/Authentication/Accounts?ReturnUrl={0}";
        private readonly IUserProfilesManager _userProfilesManager;
        private readonly IUserAccountsManager _userAccountsManager;

        public AccountSelectFilter
        (
            IUserProfilesManager userManager,
            IUserAccountsManager userAccountsManager
        )
        {
            _userProfilesManager = userManager;
            _userAccountsManager = userAccountsManager;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = _userProfilesManager.GetUser(context.HttpContext.User);
            var accounts = _userAccountsManager.GetUserAccounts(user);

            if (accounts.Count() == 1)
            {
                user.SelectedAccountId = accounts.FirstOrDefault().Id;
                await _userProfilesManager.TryUpdateUserProfile(user);
                return;
            }

            if (!string.IsNullOrEmpty(user.SelectedAccountId))
            {
                return;
            }

            var returnUrl = context.HttpContext.Request.Path.Value;
            context.Result = new RedirectResult(string.Format(Url, returnUrl));
        }
    }
}

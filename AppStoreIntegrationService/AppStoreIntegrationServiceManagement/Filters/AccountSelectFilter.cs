using AppStoreIntegrationServiceCore.DataBase.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AppStoreIntegrationServiceManagement.Filters
{
    public class AccountSelectFilter : IAuthorizationFilter
    {
        const string Url = "/Identity/Authentication/Accounts?ReturnUrl={0}";
        private readonly IUserProfilesManager _userManager;
        private readonly IUserAccountsManager _userAccountsManager;

        public AccountSelectFilter
        (
            IUserProfilesManager userManager,
            IUserAccountsManager userAccountsManager
        )
        {
            _userManager = userManager;
            _userAccountsManager = userAccountsManager;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = _userManager.GetUser(context.HttpContext.User);
            var accounts = _userAccountsManager.GetUserAccounts(user);

            if (accounts.Count() <= 1)
            {
                user.SelectedAccountId = accounts.FirstOrDefault().Id;
                _userManager.UpdateUserProfile(user);
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

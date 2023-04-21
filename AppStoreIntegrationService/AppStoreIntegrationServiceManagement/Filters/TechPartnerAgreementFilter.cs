using AppStoreIntegrationServiceCore.DataBase.Interface;
using AppStoreIntegrationServiceManagement.DataBase.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AppStoreIntegrationServiceManagement.Filters
{
    public class TechPartnerAgreementFilter : IAuthorizationFilter
    {
        const string Url = "/Identity/Authentication/Agreement?ReturnUrl={0}";
        private readonly IUserProfilesManager _userProfilesManager;
        private readonly IUserAccountsManager _userAccountsManager;
        private readonly IAccountAgreementsManager _accountAgreementsManager;
        private readonly IUserRolesManager _userRolesManager;
        private readonly IAccountsManager _accountsManager;

        public TechPartnerAgreementFilter
        (
            IUserProfilesManager userProfilesManager,
            IUserAccountsManager userAccountsManager,
            IAccountAgreementsManager accountAgreementsManager,
            IUserRolesManager userRolesManager,
            IAccountsManager accountsManager
        )
        {
            _userProfilesManager = userProfilesManager;
            _userAccountsManager = userAccountsManager;
            _accountAgreementsManager = accountAgreementsManager;
            _userRolesManager = userRolesManager;
            _accountsManager = accountsManager;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = _userProfilesManager.GetUser(context.HttpContext.User);
            var role = _userRolesManager.GetRoleByName("System Administrator");
            var account = _accountsManager.GetAccountById(user.SelectedAccountId);
            var userRole = _userAccountsManager.GetUserRoleForAccount(user, account);

            if (userRole.Equals(role) || _accountAgreementsManager.HasAggreement(user, account))
            {
                return;
            }

            var returnUrl = context.HttpContext.Request.Path.Value;
            context.Result = new RedirectResult(string.Format(Url, returnUrl));
        }
    }
}

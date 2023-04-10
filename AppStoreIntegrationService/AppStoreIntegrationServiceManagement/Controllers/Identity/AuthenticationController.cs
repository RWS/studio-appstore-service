using AppStoreIntegrationServiceCore.DataBase.Interface;
using AppStoreIntegrationServiceCore.DataBase.Models;
using AppStoreIntegrationServiceManagement.DataBase.Interface;
using AppStoreIntegrationServiceManagement.Filters;
using AppStoreIntegrationServiceManagement.Model;
using AppStoreIntegrationServiceManagement.Model.Identity;
using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppStoreIntegrationServiceManagement.Controllers.Identity
{
    [Area("Identity")]
    [Authorize]
    public class AuthenticationController : CustomController
    {
        private readonly IAccountAgreementsManager _accountAgreementsManager;
        private readonly IUserProfilesManager _userProfilesManager;
        private readonly IUserAccountsManager _userAccountsManager;
        private readonly IAccountsManager _accountsManager;

        public AuthenticationController
        (
            IAccountAgreementsManager accountAgreements,
            IUserProfilesManager userProfilesManager,
            IUserAccountsManager userAccountsManager,
            IAccountsManager accountsManager,
            IUserSeed userSeed
        ) : base(userProfilesManager, userAccountsManager, accountsManager)
        {
            userSeed.EnsureAdminExistance();
            _userProfilesManager = userProfilesManager;
            _accountAgreementsManager = accountAgreements;
            _accountsManager = accountsManager;
            _userAccountsManager = userAccountsManager;
        }

        [AllowAnonymous]
        [Route("/Account/Login")]
        public async Task Login()
        {
            var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
                .WithRedirectUri(GetUrlBase() + "/Plugins")
                .Build();

            await HttpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
        }

        public IActionResult Accounts(string returnUrl = null)
        {
            var user = _userProfilesManager.GetUser(ExtendedUser);

            return View(new AccountsModel
            {
                Accounts = _userAccountsManager.GetUserAccounts(user),
                ReturnUrl = returnUrl
            });
        }

        [HttpPost]
        public async Task<IActionResult> SelectAccount(AccountsModel model)
        {
            var user = _userProfilesManager.GetUser(ExtendedUser);
            user.SelectedAccountId = model.SelectedAccountId;
            user.RememberAccount = model.RememberMyChoice;
            await _userProfilesManager.UpdateUserProfile(user);
            return Redirect(model.ReturnUrl ?? "/Plugins");
        }

        [AccountSelect]
        public IActionResult Agreement(string returnUrl = null)
        {
            return View("Agreement", (false, returnUrl ?? "/Plugins"));
        }

        [AccountSelect]
        [HttpPost]
        public IActionResult ConsentAgreement(bool acceptedAgreement)
        {
            if (!acceptedAgreement)
            {
                TempData["StatusMessage"] = "Warning! You cannot proceed without accepting the agreement!";
                return Content("/Identity/Authentication/Agreement");
            }

            var user = _userProfilesManager.GetUser(ExtendedUser);
            _accountAgreementsManager.Add(new AccountAgreement
            {
                AccountId = _accountsManager.GetAccountById(user.SelectedAccountId).Id,
                Id = Guid.NewGuid().ToString(),
                UserProfileId = user.Id,
            });

            return Content("/Plugins");
        }

        public async Task Logout()
        {
            var user = _userProfilesManager.GetUser(User);

            if (!user.RememberAccount)
            {
                user.SelectedAccountId = null;
                await _userProfilesManager.UpdateUserProfile(user);
            }

            var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
                .WithRedirectUri(Url.Action("Login", "Authentication"))
                .Build();

            await HttpContext.SignOutAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}

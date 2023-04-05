using AppStoreIntegrationServiceCore.DataBase.Interface;
using AppStoreIntegrationServiceCore.DataBase.Models;
using AppStoreIntegrationServiceManagement.Areas.Identity.Data;
using AppStoreIntegrationServiceManagement.Filters;
using AppStoreIntegrationServiceManagement.Model;
using AppStoreIntegrationServiceManagement.Model.Identity;
using AppStoreIntegrationServiceManagement.Model.Identity.Interface;
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
        private readonly IUserProfilesManager _userProfilesManager;
        private readonly IAccountAgreementsManager _accountAgreements;
        private readonly IAccountsManager _accountsManager;

        public AuthenticationController
        (
            IUserSeed userSeed,
            IUserProfilesManager userProfilesManager,
            IAccountAgreementsManager accountAgreements,
            IAccountsManager accountsManager
        )
        {
            userSeed.EnsureAdminExistance();
            _userProfilesManager = userProfilesManager;
            _accountAgreements = accountAgreements;
            _accountsManager = accountsManager;
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
            var user = UserManager.GetUser(ExtendedUser);

            return View(new AccountsModel
            {
                Accounts = UserAccountsManager.GetUserAccounts(user),
                ReturnUrl = returnUrl
            });
        }

        [HttpPost]
        public IActionResult SelectAccount(AccountsModel model)
        {
            var user = UserManager.GetUser(ExtendedUser);
            user.SelectedAccountId = model.SelectedAccountId;
            user.RememberAccount = model.RememberMyChoice;
            UserManager.UpdateUserProfile(user);
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
            _accountAgreements.Add(new AccountAgreement
            {
                AccountId = _accountsManager.GetAccountById(user.SelectedAccountId).Id,
                Id = Guid.NewGuid().ToString(),
                UserProfileId = user.Id,
            });

            return Content("/Plugins");
        }

        public async Task Logout()
        {
            var user = UserManager.GetUser(User);

            if (!user.RememberAccount)
            {
                user.SelectedAccountId = null;
                _userProfilesManager.UpdateUserProfile(user);
            }

            var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
                .WithRedirectUri(Url.Action("Login", "Authentication"))
                .Build();

            await HttpContext.SignOutAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}

using AppStoreIntegrationServiceManagement.Areas.Identity.Data;
using AppStoreIntegrationServiceManagement.Model.DataBase;
using AppStoreIntegrationServiceManagement.Model.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AppStoreIntegrationServiceManagement.Controllers.Identity
{
    [Area("Identity")]
    public class AuthenticationController : Controller
    {
        private readonly SignInManager<IdentityUserExtended> _signInManager;
        private readonly UserManager<IdentityUserExtended> _userManager;
        private readonly UserAccountsManager _userAccountManager;

        public AuthenticationController
        (
            SignInManager<IdentityUserExtended> signInManager, 
            IUserSeed userSeed,
            UserManager<IdentityUserExtended> userManager,
            UserAccountsManager userAccountsManager
        )
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userAccountManager = userAccountsManager;
            userSeed.EnsureAdminExistance();
        }

        public async Task<IActionResult> Login(string returnUrl = null)
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            return View(new LoginModel
            {
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList(),
                ReturnUrl = returnUrl ?? Url.Content("~/Plugins")
            });
        }

        [HttpPost]
        public async Task<IActionResult> PostLogin(LoginModel loginModel, string returnUrl = null)
        {
            var user = await _userManager.FindByEmailAsync(loginModel.Input.Email);

            if (user == null)
            {
                TempData["StatusMessage"] = "Error! This e-mail address doesn't exist in our database!";
                return View("Login", loginModel);
            }

            if (!_userAccountManager.HasAssociatedAccounts(user))
            {
                TempData["StatusMessage"] = "Error! You are no longer part of AppStore!";
                return View("Login", loginModel);
            }

            var result = await _signInManager.PasswordSignInAsync(user, loginModel.Input.Password, loginModel.Input.RememberMe, false);
            user.SelectedAccountId = null;
            await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return Redirect(returnUrl);
            }

            TempData["StatusMessage"] = "Error! Something went wrong!";
            return View("Login", loginModel);
        }

        [Authorize]
        public async Task<IActionResult> Accounts(string returnUrl = null)
        {
            var user = await _userManager.GetUserAsync(User);
            var accounts = _userAccountManager.GetUserParentAccounts(user);
            return View((accounts, returnUrl));
        }

        [HttpPost]
        public async Task<IActionResult> SelectAccount(string accountId, string returnUrl = null)
        {
            var user = await _userManager.GetUserAsync(User);
            user.SelectedAccountId = accountId;
            await _userManager.UpdateAsync(user);
            return Redirect(returnUrl ?? "/Plugins");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            var user = await _userManager.GetUserAsync(User);
            user.SelectedAccountId = null;
            await _userManager.UpdateAsync(user);
            await _signInManager.SignOutAsync();
            TempData["StatusMessage"] = "Success! You were logged out!";
            return RedirectToAction("Login");
        }
    }
}

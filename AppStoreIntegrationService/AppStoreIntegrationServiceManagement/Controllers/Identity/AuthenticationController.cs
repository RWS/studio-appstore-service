using AppStoreIntegrationServiceManagement.Areas.Identity.Data;
using AppStoreIntegrationServiceManagement.Model;
using AppStoreIntegrationServiceManagement.Model.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AppStoreIntegrationServiceManagement.Controllers.Identity
{
    [Area("Identity")]
    public class AuthenticationController : CustomController
    {
        public AuthenticationController(IUserSeed userSeed)
        {
            userSeed.EnsureAdminExistance();
        }

        public async Task<IActionResult> Login(string returnUrl = null)
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            return View(new LoginModel
            {
                ExternalLogins = (await SignInManager.GetExternalAuthenticationSchemesAsync()).ToList(),
                ReturnUrl = returnUrl ?? Url.Content("~/Plugins")
            });
        }

        [HttpPost]
        public async Task<IActionResult> PostLogin(LoginModel loginModel, string returnUrl = null)
        {
            var user = await UserManager.FindByEmailAsync(loginModel.Input.Email);

            if (user == null)
            {
                TempData["StatusMessage"] = "Error! This e-mail address doesn't exist in our database!";
                return View("Login", loginModel);
            }

            if (!UserAccountsManager.HasAssociatedAccounts(user))
            {
                TempData["StatusMessage"] = "Error! You are no longer part of AppStore!";
                return View("Login", loginModel);
            }

            var result = await SignInManager.PasswordSignInAsync(user, loginModel.Input.Password, loginModel.Input.RememberMe, false);
            user.SelectedAccountId = null;
            await UserManager.UpdateAsync(user);

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
            var user = await UserManager.GetUserAsync(User);
            var accounts = UserAccountsManager.GetUserParentAccounts(user);
            return View((accounts, returnUrl));
        }

        [HttpPost]
        public async Task<IActionResult> SelectAccount(string accountId, string returnUrl = null)
        {
            var user = await UserManager.GetUserAsync(User);
            user.SelectedAccountId = accountId;
            await UserManager.UpdateAsync(user);
            return Redirect(returnUrl ?? "/Plugins");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            var user = await UserManager.GetUserAsync(User);
            user.SelectedAccountId = null;
            await UserManager.UpdateAsync(user);
            await SignInManager.SignOutAsync();
            TempData["StatusMessage"] = "Success! You were logged out!";
            return RedirectToAction("Login");
        }
    }
}

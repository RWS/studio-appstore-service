using AppStoreIntegrationServiceManagement.Areas.Identity.Data;
using AppStoreIntegrationServiceManagement.Model.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AppStoreIntegrationServiceManagement.Controllers.Identity
{
    [Area("Identity")]
    public class AuthenticationController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;

        public AuthenticationController(SignInManager<IdentityUser> signInManager, IUserSeed userSeed)
        {
            _signInManager = signInManager;
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
            if (!ModelState.IsValid)
            {
                TempData["StatusMessage"] = "Error! Model state is invalid!";
                return View("Login", loginModel);
            }

            var result = await _signInManager.PasswordSignInAsync(loginModel.Input.UserName, loginModel.Input.Password, loginModel.Input.RememberMe, false);
            if (result.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }

            TempData["StatusMessage"] = "Error! Something went wrong!";
            return View("Login", loginModel);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["StatusMessage"] = "Success! You were logged out!";
            return RedirectToAction("Login");
        }
    }
}

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
        private readonly ILogger<LoginModel> _logger;

        public AuthenticationController(SignInManager<IdentityUser> signInManager, ILogger<LoginModel> logger, IUserSeed userSeed)
        {
            _signInManager = signInManager;
            _logger = logger;
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
        public async Task<IActionResult> PostLogin(LoginModel loginModel, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(loginModel.Input.UserName, loginModel.Input.Password, loginModel.Input.RememberMe,
                    lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return LocalRedirect(returnUrl);
                }
                //if (result.RequiresTwoFactor)
                //{
                //    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                //}
                loginModel.ErrorMessage = "Error! Invalid login attempt.";
                return View("Login", loginModel);
            }

            loginModel.ErrorMessage = "Error! Something went wrong!";
            return View("Login", loginModel);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
    }
}

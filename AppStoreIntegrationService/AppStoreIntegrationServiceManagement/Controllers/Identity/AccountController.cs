using AppStoreIntegrationServiceManagement.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AppStoreIntegrationServiceManagement.Controllers.Identity
{
    [Area("Identity")]
    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<LoginModel> _logger;

        public AccountController(SignInManager<IdentityUser> signInManager, ILogger<LoginModel> logger, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;

            if (!userManager.Users.Any())
            {
                if (!roleManager.Roles.Any())
                {
                    roleManager.CreateAsync(new IdentityRole
                    {
                        Name = "Administrator",
                        Id = "1"
                    }).Wait();
                    roleManager.CreateAsync(new IdentityRole
                    {
                        Name = "StandardUser",
                        Id = "2"
                    }).Wait();
                }

                var defaultAdminUser = new IdentityUser { UserName = "Admin", Email = "admin@sdl.com" };
                userManager.CreateAsync(defaultAdminUser, "administrator").Wait();

                userManager.AddToRoleAsync(defaultAdminUser, "Administrator").Wait();
                _signInManager.SignInAsync(defaultAdminUser, false);
            }

            _logger = logger;
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
                loginModel.ErrorMessage = "Error!Invalid login attempt.";
                return View("Login", loginModel);
            }

            loginModel.ErrorMessage = "Error!Something went wrong!";
            return View("Login", loginModel);
        }

        public async Task<IActionResult> Register(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/Plugins");
            return View(new RegisterModel
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            });
        }

        [HttpPost]
        public async Task<IActionResult> PostRegister(RegisterModel registerModel, string redirectUrl)
        {
            registerModel.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = registerModel.Input.UserName, Email = registerModel.Input.UserName };
                var result = await _userManager.CreateAsync(user, registerModel.Input.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, registerModel.Input.UserRole);
                    _logger.LogInformation("User created a new account with password.");

                    return LocalRedirect(redirectUrl);
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return View();
        }

        [HttpPost]
        public async Task Logout()
        {
            if (_signInManager.IsSignedIn(User))
            {
                await _signInManager.SignOutAsync();
            }
        }
    }
}

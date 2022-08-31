using AppStoreIntegrationServiceManagement.Model.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AppStoreIntegrationServiceManagement.Controllers.Identity
{
    [Area("Identity")]
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<ChangePasswordModel> _logger;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, ILogger<ChangePasswordModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            return View(await LoadAsync(user));
        }

        [HttpPost]
        public async Task<IActionResult> Update(ProfileModel profileModel)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index");
            }

            if (user.UserName != profileModel.Username)
            {
                await _userManager.SetUserNameAsync(user, profileModel.Username);
            }

            var oldUserRole = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
            if (oldUserRole != profileModel.UserRole)
            {
                await _userManager.RemoveFromRoleAsync(user, oldUserRole);
                await _userManager.AddToRoleAsync(user, profileModel.UserRole);
            }

            profileModel.StatusMessage = "Your profile has been updated";
            return RedirectToAction("Profile");
        }

        public async Task<IActionResult> ChangePassword()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var hasPassword = await _userManager.HasPasswordAsync(user);
            if (!hasPassword)
            {
                return RedirectToPage("./SetPassword");
            }

            return View(new ChangePasswordModel());
        }

        public async Task<IActionResult> PostChangePassword(ChangePasswordModel changePasswordModel)
        {
            if (!ModelState.IsValid)
            {
                return View(new ChangePasswordModel());
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, changePasswordModel.Input.OldPassword, changePasswordModel.Input.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return RedirectToAction("ChangePassword");
            }

            await _signInManager.RefreshSignInAsync(user);
            _logger.LogInformation("User changed their password successfully.");
            changePasswordModel.StatusMessage = "Your password has been changed.";

            return RedirectToAction("ChangePassword");
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Register()
        {
            return View(new RegisterModel
            {
                ReturnUrl = Url.Content("~/Plugins"),
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            });
        }
        
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> PostRegister(RegisterModel registerModel)
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

                    return LocalRedirect(registerModel.ReturnUrl);
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return View();
        }

        private async Task<ProfileModel> LoadAsync(IdentityUser user)
        {
            return new ProfileModel
            {
                Username = await _userManager.GetUserNameAsync(user),
                IsAdmin = await _userManager.IsInRoleAsync(user, "Administrator")
            };
        }
    }
}

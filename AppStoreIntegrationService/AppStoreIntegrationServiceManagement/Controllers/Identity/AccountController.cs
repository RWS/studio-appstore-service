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

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return View(new ProfileModel { StatusMessage = "Error! User is null!" });
            }

            return View(await LoadAsync(user, ""));
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Update(ProfileModel profileModel)
        {
            var user = await _userManager.GetUserAsync(User);
            var (newUsername, newRole) = (profileModel.Username, profileModel.UserRole);
            if (user == null && !ModelState.IsValid)
            {
                return View("Profile", new ProfileModel { StatusMessage = "Error! User is null or model state is invalid!" });
            }

            if (!string.IsNullOrEmpty(newUsername))
            {
                await _userManager.SetUserNameAsync(user, newUsername);
            }

            if (!string.IsNullOrEmpty(newRole))
            {
                await _userManager.RemoveFromRoleAsync(user, (await _userManager.GetRolesAsync(user))[0]);
                await _userManager.AddToRoleAsync(user, newRole);
            }

            return View("Profile", await LoadAsync(user, "Success! Profile was updated!"));
        }

        public async Task<IActionResult> ChangePassword()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return View("ChangePassword", new ChangePasswordModel { StatusMessage = "Error! User is null!" });
            }

            return View(new ChangePasswordModel());
        }

        public async Task<IActionResult> PostChangePassword(ChangePasswordModel changePasswordModel)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null && !ModelState.IsValid)
            {
                return View("ChangePassword", new ChangePasswordModel { StatusMessage = "Error! User is null or model state is invalid!" });
            }

            await _userManager.ChangePasswordAsync(user, changePasswordModel.Input.OldPassword, changePasswordModel.Input.NewPassword);
            await _signInManager.RefreshSignInAsync(user);
            return View("ChangePassword", new ChangePasswordModel { StatusMessage = "Success! Password was updated!" });
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
                    return LocalRedirect(registerModel.ReturnUrl);
                }
            }

            return View("Register", new RegisterModel { StatusMessage = "Error! Something went wrong!" });
        }

        private async Task<ProfileModel> LoadAsync(IdentityUser user, string message)
        {
            return new ProfileModel
            {
                Username = await _userManager.GetUserNameAsync(user),
                UserRole = (await _userManager.GetRolesAsync(user)).FirstOrDefault(),
                StatusMessage = message
            };
        }
    }
}

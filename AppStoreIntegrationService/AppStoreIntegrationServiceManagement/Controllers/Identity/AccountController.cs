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
                TempData["StatusMessage"] = "Error! User is null!";
                return View();
            }

            return View(await LoadAsync(user));
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Update(ProfileModel profileModel)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null && !ModelState.IsValid)
            {
                TempData["StatusMessage"] = "Error! User is null or model state is invalid!";
                return RedirectToAction("Profile");
            }

            var newUsername = profileModel.Username;
            if (!string.IsNullOrEmpty(newUsername))
            {
                await _userManager.SetUserNameAsync(user, newUsername);
                TempData["StatusMessage"] = "Success! Profile was updated!";
                return RedirectToAction("Profile");
            }

            TempData["StatusMessage"] = "Error! Username cannot be null!";
            return RedirectToAction("Profile");
        }

        public async Task<IActionResult> ChangePassword()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["StatusMessage"] = "Error! User is null!";
                return View();
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PostChangePassword(ChangePasswordModel changePasswordModel)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null && !ModelState.IsValid)
            {
                TempData["StatusMessage"] = "Error! User is null or model state is invalid!";
                return RedirectToAction("ChangePassword");
            }

            await _userManager.ChangePasswordAsync(user, changePasswordModel.Input.OldPassword, changePasswordModel.Input.NewPassword);
            await _signInManager.RefreshSignInAsync(user);
            TempData["StatusMessage"] = "Success! Password was updated!";
            return RedirectToAction("ChangePassword");
        }

        private async Task<ProfileModel> LoadAsync(IdentityUser user)
        {
            var username = await _userManager.GetUserNameAsync(user);
            var role = (await _userManager.GetRolesAsync(user))[0];

            return new ProfileModel
            {
                Username = username,
                UserRole = role,
                IsUsernameEnabled = username != "Admin" && role == "Administrator"
            };
        }
    }
}

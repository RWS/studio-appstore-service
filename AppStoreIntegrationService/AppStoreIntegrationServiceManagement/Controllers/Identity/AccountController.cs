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

        public async Task<IActionResult> Profile(string statusMessage)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return View(new ProfileModel { StatusMessage = "Error! User is null!" });
            }

            return View(await LoadAsync(user, statusMessage));
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Update(ProfileModel profileModel)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null && !ModelState.IsValid)
            {
                return RedirectToAction("Profile", new { statusMessage = "Error! User is null or model state is invalid!" });
            }

            var newUsername = profileModel.Username;
            if (!string.IsNullOrEmpty(newUsername))
            {
                await _userManager.SetUserNameAsync(user, newUsername);
                return RedirectToAction("Profile", new { statusMessage = "Success! Profile was updated!" });
            }

            return RedirectToAction("Profile", new { statusMessage = "Error! Username cannot be null!" });
        }

        public async Task<IActionResult> ChangePassword(string statusMessage)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return View(new ChangePasswordModel { StatusMessage = "Error! User is null!" });
            }

            return View(new ChangePasswordModel { StatusMessage = statusMessage });
        }

        public async Task<IActionResult> PostChangePassword(ChangePasswordModel changePasswordModel)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null && !ModelState.IsValid)
            {
                return RedirectToAction("ChangePassword", new { statusMessage = "Error! User is null or model state is invalid!" });
            }

            await _userManager.ChangePasswordAsync(user, changePasswordModel.Input.OldPassword, changePasswordModel.Input.NewPassword);
            await _signInManager.RefreshSignInAsync(user);
            return RedirectToAction("ChangePassword", new { statusMessage = "Success! Password was updated!" });
        }

        private async Task<ProfileModel> LoadAsync(IdentityUser user, string message)
        {
            var username = await _userManager.GetUserNameAsync(user);
            var role = (await _userManager.GetRolesAsync(user))[0];

            return new ProfileModel
            {
                Username = username,
                UserRole = role,
                IsUsernameEnabled = username != "Admin" && role == "Administrator",
                StatusMessage = message
            };
        }
    }
}

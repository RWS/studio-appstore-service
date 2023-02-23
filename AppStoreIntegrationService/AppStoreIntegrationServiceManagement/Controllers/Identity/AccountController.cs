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
        private readonly UserManager<IdentityUserExtended> _userManager;
        private readonly SignInManager<IdentityUserExtended> _signInManager;

        public AccountController(UserManager<IdentityUserExtended> userManager, SignInManager<IdentityUserExtended> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Profile(string id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var wantedUser = await _userManager.FindByIdAsync(id);

            if (!TryValidate(currentUser, id, wantedUser, true, out IActionResult result))
            {
                return result;
            }

            if (string.IsNullOrEmpty(id) || currentUser == wantedUser)
            {
                return View(await LoadAsync(currentUser, true));
            }

            if (User.IsInRole("Administrator") && wantedUser.UserName != "Admin")
            {
                return View(await LoadAsync(wantedUser, false));
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Update(ProfileModel profileModel, string id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var wantedUser = await _userManager.FindByIdAsync(id);
            List<IdentityResult> results = new();

            if (!TryValidate(currentUser, id, wantedUser, true, out IActionResult result))
            {
                return result;
            }

            if (string.IsNullOrEmpty(id) || currentUser == wantedUser)
            {
                results = new List<IdentityResult>
                {
                    await _userManager.SetUserNameAsync(currentUser, profileModel.UserName),
                    await _userManager.SetEmailAsync(currentUser, profileModel.UserName)
                };

                if (results.Any(x => !x.Succeeded))
                {
                    TempData["StatusMessage"] = string.Format("Error! {0}", results.First(x => !x.Succeeded).Errors.First().Description);
                    return RedirectToAction("Profile");
                }

                TempData["StatusMessage"] = "Success! Your profile was updated!";
                return RedirectToAction("Profile");
            }

            if (profileModel.UserName != wantedUser.UserName)
            {
                results.Add(await _userManager.SetUserNameAsync(wantedUser, profileModel.UserName));
            }

            if (profileModel.UserName != wantedUser.UserName)
            {
                results.Add(await _userManager.SetEmailAsync(wantedUser, profileModel.UserName));
            }

            var oldRole = (await _userManager.GetRolesAsync(wantedUser))[0];
            if (profileModel.UserRole != oldRole)
            {
                await _userManager.RemoveFromRoleAsync(wantedUser, oldRole);
                await _userManager.AddToRoleAsync(wantedUser, profileModel.UserRole);
            }

            if (results.Any(x => !x.Succeeded))
            {
                TempData["StatusMessage"] = string.Format("Error! {0}", results.First(x => !x.Succeeded).Errors.First().Description);
                return RedirectToAction("Profile");
            }

            TempData["StatusMessage"] = string.Format("Success! {0}'s profile was updated!", wantedUser.UserName);
            return RedirectToAction("Profile", new { id });
        }

        public async Task<IActionResult> ChangePassword(string id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var wantedUser = await _userManager.FindByIdAsync(id);

            if (!TryValidate(currentUser, id, wantedUser, true, out IActionResult result))
            {
                return result;
            }

            if (string.IsNullOrEmpty(id) || currentUser == wantedUser)
            {
                return View(new ChangePasswordModel { IsCurrentUserSelected = true });
            }

            if (User.IsInRole("Administrator") && wantedUser.UserName != "Admin")
            {
                return View(new ChangePasswordModel
                {
                    Id = wantedUser.Id,
                    Username = wantedUser.UserName
                });
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> PostChangePassword(ChangePasswordModel model, string id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var wantedUser = await _userManager.FindByIdAsync(id);

            if (!TryValidate(currentUser, id, wantedUser, model.Input.OldPassword != null, out IActionResult result))
            {
                return result;
            }

            if (string.IsNullOrEmpty(id) || currentUser == wantedUser)
            {
                await _userManager.ChangePasswordAsync(currentUser, model.Input.OldPassword, model.Input.NewPassword);
                await _signInManager.RefreshSignInAsync(currentUser);
                TempData["StatusMessage"] = "Success! Your password was changed!";
                return RedirectToAction("Profile");
            }

            await _userManager.RemovePasswordAsync(wantedUser);
            await _userManager.AddPasswordAsync(wantedUser, model.Input.NewPassword);
            TempData["StatusMessage"] = string.Format("Success! {0}'s password was changed!", wantedUser.UserName);
            return RedirectToAction("Profile", new { id });
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Users()
        {
            var users = new List<UserInfoModel>();
            foreach (var user in _userManager.Users.ToList())
            {
                users.Add(new UserInfoModel
                {
                    Id = user.Id,
                    Name = user.UserName,
                    Role = (await _userManager.GetRolesAsync(user))[0],
                    IsCurrentUser = user == (await _userManager.GetUserAsync(User))
                });
            }

            return View(users);
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Register()
        {
            return View(new RegisterModel
            {
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            });
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> PostRegister(RegisterModel registerModel)
        {
            registerModel.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            var user = new IdentityUserExtended { UserName = registerModel.Input.UserName };
            var result = await _userManager.CreateAsync(user, registerModel.Input.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, registerModel.Input.UserRole);
                TempData["StatusMessage"] = string.Format("Success! {0} was added!", user.UserName);
                return RedirectToAction("Users");
            }

            TempData["StatusMessage"] = string.Format("Error! {0}", result.Errors.First().Description);
            return RedirectToAction("Register");
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            var currentUser = await _userManager.GetUserAsync(User);

            if (!TryValidate(user, null, null, false, out IActionResult result))
            {
                return result;
            }

            if (user == currentUser)
            {
                TempData["StatusMessage"] = "Error! You cannot delete your account!";
                return Content("");
            }

            await _userManager.DeleteAsync(user);
            TempData["StatusMessage"] = string.Format("Success! {0} was deleted!", user.UserName);
            return Content("");
        }

        private bool TryValidate(IdentityUserExtended currentUser, string id, IdentityUserExtended wantedUser, bool checkModelState, out IActionResult result)
        {
            if (currentUser == null)
            {
                result = NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
                return false;
            }

            if (checkModelState && !ModelState.IsValid)
            {
                TempData["StatusMessage"] = "Error! Model state is invalid!";
                result = RedirectToAction("Profile");
                return false;
            }

            if (id != null && wantedUser == null)
            {
                result = NotFound($"Unable to load user with ID '{id}'.");
                return false;
            }

            result = null;
            return true;
        }

        private async Task<ProfileModel> LoadAsync(IdentityUserExtended user, bool isCurrentUserProfile)
        {
            var username = await _userManager.GetUserNameAsync(user);
            var role = (await _userManager.GetRolesAsync(user))[0];

            return new ProfileModel
            {
                UserName = user.UserName,
                UserRole = role,
                IsUsernameEditable = !isCurrentUserProfile || (username != "Admin" && role == "Administrator"),
                IsUserRoleEditable = !isCurrentUserProfile,
                Id = isCurrentUserProfile ? null : user.Id
            };
        }
    }
}

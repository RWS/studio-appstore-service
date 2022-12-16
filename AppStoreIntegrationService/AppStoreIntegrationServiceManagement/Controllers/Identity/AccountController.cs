using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceManagement.Model;
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

            if (!TryValidate(currentUser, id, wantedUser, true, out IActionResult result))
            {
                return result;
            }

            var (newUsername, newRole, newEmail) = (profileModel.Username, profileModel.UserRole, profileModel.Email);
            if (string.IsNullOrEmpty(id) || currentUser == wantedUser)
            {
                var identityResult = await _userManager.SetUserNameAsync(currentUser, newUsername);
                if (!identityResult.Succeeded)
                {
                    TempData["StatusMessage"] = string.Format("Error! {0}", identityResult.Errors.First().Description);
                    return RedirectToAction("Profile");
                }

                identityResult = await _userManager.SetEmailAsync(currentUser, newEmail);
                if (!identityResult.Succeeded)
                {
                    TempData["StatusMessage"] = string.Format("Error! {0}", identityResult.Errors.First().Description);
                    return RedirectToAction("Profile");
                }

                TempData["StatusMessage"] = "Success! Your profile was updated!";
                return RedirectToAction("Profile");
            }

            var oldUsername = wantedUser.UserName;
            if (newUsername != wantedUser.UserName)
            {
                var identityResult = await _userManager.SetUserNameAsync(wantedUser, newUsername);
                if (!identityResult.Succeeded)
                {
                    TempData["StatusMessage"] = string.Format("Error! {0}", identityResult.Errors.First().Description);
                    return RedirectToAction("Profile", new { id });
                }
            }

            if (newEmail != wantedUser.Email)
            {
                var identityResult = await _userManager.SetEmailAsync(wantedUser, newEmail);
                if (!identityResult.Succeeded)
                {
                    TempData["StatusMessage"] = string.Format("Error! {0}", identityResult.Errors.First().Description);
                    return RedirectToAction("Profile", new { id });
                }
            }

            var oldRole = (await _userManager.GetRolesAsync(wantedUser))[0];
            if (newRole != oldRole)
            {
                await _userManager.RemoveFromRoleAsync(wantedUser, oldRole);
                await _userManager.AddToRoleAsync(wantedUser, newRole);
            }

            TempData["StatusMessage"] = string.Format("Success! {0}'s profile was updated!", oldUsername);
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
                    Email = user.Email,
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
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = registerModel.Input.UserName, Email = registerModel.Input.UserName };
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

            TempData["StatusMessage"] = "Error! Model state is invalid!";
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

        [Route("[controller]/[action]/{redirectUrl?}/{currentPage?}")]
        public async Task<IActionResult> GoToPage(ProfileModel profileModel, ChangePasswordModel passwordModel, RegisterModel registerModel, string redirectUrl, string currentPage)
        {
            var editedUser = await _userManager.FindByIdAsync(profileModel.Id);
            var currentUser = await _userManager.GetUserAsync(User);
            var user = editedUser ?? currentUser;
            redirectUrl = redirectUrl.Replace('.', '/');

            if (currentPage == "Profile" && await IsSavedProfile(profileModel, user) ||
                currentPage == "ChangePassword" && passwordModel.Input.IsEmpty() ||
                currentPage == "Register" && registerModel.Input.IsEmpty())
            {
                return Content(redirectUrl);
            }

            var modalDetails = new ModalMessage
            {
                RequestPage = $"{redirectUrl}",
                ModalType = ModalType.WarningMessage,
                Title = "Unsaved changes!",
                Message = string.Format("Discard changes for {0}?", user.UserName)
            };

            return PartialView("_ModalPartial", modalDetails);
        }

        private bool TryValidate(IdentityUser currentUser, string id, IdentityUser wantedUser, bool checkModelState, out IActionResult result)
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

        private async Task<ProfileModel> LoadAsync(IdentityUser user, bool isCurrentUserProfile)
        {
            var username = await _userManager.GetUserNameAsync(user);
            var role = (await _userManager.GetRolesAsync(user))[0];

            return new ProfileModel
            {
                Username = username,
                Email= user.Email,
                UserRole = role,
                IsUsernameEditable = !isCurrentUserProfile || (username != "Admin" && role == "Administrator"),
                IsUserRoleEditable = !isCurrentUserProfile,
                Id = isCurrentUserProfile ? null : user.Id
            };
        }

        private async Task<bool> IsSavedProfile(ProfileModel model, IdentityUser user)
        {
            var (oldUsername, oldUserRole) = (user.UserName, (await _userManager.GetRolesAsync(user))[0]);
            var (newUsername, newUserRole) = (model.Username, model.UserRole);

            return (newUserRole == null, newUsername == null) switch
            {
                (true, true) => true,
                (true, false) => oldUsername == newUsername,
                _ => oldUsername == newUsername && oldUserRole == newUserRole
            };
        }
    }
}

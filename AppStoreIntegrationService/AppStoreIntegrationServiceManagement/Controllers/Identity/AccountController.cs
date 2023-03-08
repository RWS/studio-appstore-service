using AppStoreIntegrationServiceManagement.Filters;
using AppStoreIntegrationServiceManagement.Model;
using AppStoreIntegrationServiceManagement.Model.DataBase;
using AppStoreIntegrationServiceManagement.Model.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppStoreIntegrationServiceManagement.Controllers.Identity
{
    [Area("Identity")]
    [Authorize]
    [AccountSelected]
    public class AccountController : CustomController
    {
        private readonly UserManager<IdentityUserExtended> _userManager;
        private readonly SignInManager<IdentityUserExtended> _signInManager;
        private readonly UserAccountsManager _userAccountsManager;
        private readonly AccountsManager _accountsManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController
        (
            UserManager<IdentityUserExtended> userManager,
            SignInManager<IdentityUserExtended> signInManager,
            UserAccountsManager userAccountsManager,
            AccountsManager accountsManager,
            RoleManager<IdentityRole> roleManager
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _userAccountsManager = userAccountsManager;
            _accountsManager = accountsManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Profile(string id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var wantedUser = await _userManager.FindByIdAsync(id);

            if (!TryValidate(currentUser, id, wantedUser, out IActionResult result))
            {
                return result;
            }

            if (string.IsNullOrEmpty(id) || currentUser == wantedUser)
            {
                return View(Load(currentUser, true));
            }

            if (ExtendedUser.IsInRoles("Administrator") && !wantedUser.IsBuiltInAdmin)
            {
                return View(Load(wantedUser, false));
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Update(ProfileModel profileModel, string id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var wantedUser = await _userManager.FindByIdAsync(id);
            List<IdentityResult> results = new();

            if (!TryValidate(currentUser, id, wantedUser, out IActionResult result))
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

            results = new List<IdentityResult>
            {
                await _userManager.SetUserNameAsync(wantedUser, profileModel.UserName),
                await _userManager.SetEmailAsync(wantedUser, profileModel.UserName)
            };

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

            if (!TryValidate(currentUser, id, wantedUser, out IActionResult result))
            {
                return result;
            }

            if (string.IsNullOrEmpty(id) || currentUser == wantedUser)
            {
                return View(new ChangePasswordModel { IsCurrentUserSelected = true });
            }

            if (ExtendedUser.IsInRole("Administrator") && !wantedUser.IsBuiltInAdmin)
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

            if (!TryValidate(currentUser, id, wantedUser, out IActionResult result))
            {
                return result;
            }

            if (string.IsNullOrEmpty(id) || currentUser == wantedUser)
            {
                var identityResult = await _userManager.ChangePasswordAsync(currentUser, model.Input.OldPassword, model.Input.NewPassword);
                if (identityResult.Succeeded)
                {
                    await _signInManager.RefreshSignInAsync(currentUser);
                    TempData["StatusMessage"] = "Success! Your password was changed!";
                }

                TempData["StatusMessage"] = string.Format("Error! {0}", identityResult.Errors.First().Description);
                return RedirectToAction("Profile");
            }

            var results = new List<IdentityResult>
            {
                await _userManager.RemovePasswordAsync(wantedUser),
                await _userManager.AddPasswordAsync(wantedUser, model.Input.NewPassword)
            };

            if (results.Any(x => !x.Succeeded))
            {
                TempData["StatusMessage"] = string.Format("Error! {0}!", results.First(x => !x.Succeeded).Errors.First().Description);
            }

            TempData["StatusMessage"] = string.Format("Success! {0}'s password was changed!", wantedUser.UserName);
            return RedirectToAction("Profile", new { id });
        }

        [Owner]
        public async Task<IActionResult> Users()
        {
            var user = await _userManager.GetUserAsync(User);
            var users = _userManager.Users.ToList();
            var assignedUsers = users.Where(x => _userAccountsManager.BelongsTo(user, x));
            var allUsers = ExtendedUser.IsInRole("Administrator") ? users : Enumerable.Empty<IdentityUserExtended>();
            return View((assignedUsers.Select(x => ToUserInfo(x).Result), allUsers));
        }

        [Owner]
        public async Task<IActionResult> Accounts()
        {
            var user = await _userManager.GetUserAsync(User);
            return View(_userAccountsManager.GetUserParentAccounts(user));
        }

        [Owner]
        public async Task<IActionResult> UpdateAccount(Account account)
        {
            var user = await _userManager.GetUserAsync(User);

            if (!_userAccountsManager.IsOwner(user, account))
            {
                TempData["StatusMessage"] = "Error! You cannot edit this account!";
                return new EmptyResult();
            }

            _accountsManager.UpdateAccount(account);
            TempData["StatusMessage"] = "Success! The account was updated!";
            return new EmptyResult();
        }

        [Owner]
        public async Task<IActionResult> Register()
        {
            return View(new RegisterModel
            {
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            });
        }

        [HttpPost]
        [Owner]
        public async Task<IActionResult> PostRegister(RegisterModel registerModel)
        {
            registerModel.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            var user = new IdentityUserExtended { UserName = registerModel.UserName, Email = registerModel.Email };
            var currentUser = await _userManager.GetUserAsync(User);
            var account = _userAccountsManager.GetUserAccount(currentUser);

            var results = new List<IdentityResult>
            {
                await _userManager.CreateAsync(user, registerModel.Password),
                await _userAccountsManager.TryAddUserToAccount(user.Id, registerModel.UserRole, $"{user.UserName} Account"),
                await _userAccountsManager.TryAddUserToAccount(user.Id, registerModel.UserRole, $"{user.UserName} Account", account.AccountName),
            };

            if (results.All(x => x.Succeeded))
            {
                TempData["StatusMessage"] = string.Format("Success! {0} was added!", user.UserName);
                return RedirectToAction("Users");
            }

            TempData["StatusMessage"] = string.Format("Error! {0}", results.FirstOrDefault(x => !x.Succeeded).Errors.First().Description);
            return RedirectToAction("Register");
        }

        [RoleAuthorize("Administrator")]
        [Owner]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            var currentUser = await _userManager.GetUserAsync(User);

            if (!TryValidate(user, null, null, out IActionResult result))
            {
                return result;
            }

            if (user == currentUser)
            {
                TempData["StatusMessage"] = "Error! You cannot delete your account!";
                return Content(null);
            }

            await _userManager.DeleteAsync(user);
            _userAccountsManager.RemoveUserFromAccounts(user);
            TempData["StatusMessage"] = string.Format("Success! {0} was deleted!", user.UserName);
            return Content(null);
        }

        [Authorize]
        [Owner]
        public async Task<IActionResult> Dismiss(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            var currentUser = await _userManager.GetUserAsync(User);
            var account = _userAccountsManager.GetUserAccount(currentUser);
            var result = _userAccountsManager.RemoveUserFromAccount(user, account);

            if (result.Succeeded)
            {
                TempData["StatusMessage"] = string.Format("{0} was dismissed succesfully", user.UserName);
                return RedirectToAction("Users");
            }

            TempData["StatusMessage"] = string.Format("Error! {0}", result.Errors.First().Description);
            return RedirectToAction("Users");
        }

        [Authorize]
        [Owner]
        public async Task<IActionResult> Assign(string userId, string roleId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var account = _userAccountsManager.GetUserAccount(user);
            var currentUser = await _userManager.GetUserAsync(User);
            var entryAccount = _userAccountsManager.GetUserAccount(currentUser);
            var result = await _userAccountsManager.TryAddUserToAccount(user.Id, roleId, account.AccountName, entryAccount.AccountName);

            if (result.Succeeded)
            {
                TempData["StatusMessage"] = string.Format("Success! {0} was assigned succesfully", user.UserName);
                return Content("/Identity/Account/Users");
            }

            TempData["StatusMessage"] = result.Errors.First().Description;
            return new EmptyResult();
        }

        [Authorize]
        [Owner]
        public async Task<IActionResult> CheckUserExistance(string username)
        {
            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
            {
                return Json(new { Message = "The user does not exits!", IsErrorMessage = false });
            }

            return PartialView("_ExistentUserPartial", user.Id);
        }

        private async Task<UserInfoModel> ToUserInfo(IdentityUserExtended user)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var account = _userAccountsManager.GetUserAccount(currentUser);

            return new UserInfoModel
            {
                Id = user.Id,
                Name = user.UserName,
                Role = _userAccountsManager.GetUserRoleForAccount(user, account).Result.Name,
                IsCurrentUser = user == currentUser,
                IsBuiltInAdmin = user.IsBuiltInAdmin,
                IsOwner = _userAccountsManager.IsOwner(user, account)
            };
        }

        private bool TryValidate(IdentityUserExtended currentUser, string id, IdentityUserExtended wantedUser, out IActionResult result)
        {
            if (currentUser == null)
            {
                result = NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
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

        private static ProfileModel Load(IdentityUserExtended user, bool isCurrentUserProfile)
        {
            return new ProfileModel
            {
                UserName = user.UserName,
                Email = user.Email,
                IsUsernameEditable = !isCurrentUserProfile || !user.IsBuiltInAdmin,
                Id = isCurrentUserProfile ? null : user.Id
            };
        }
    }
}

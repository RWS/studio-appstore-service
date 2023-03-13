using AppStoreIntegrationServiceManagement.Filters;
using AppStoreIntegrationServiceManagement.Model;
using AppStoreIntegrationServiceManagement.Model.DataBase;
using AppStoreIntegrationServiceManagement.Model.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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

        public AccountController
        (
            UserManager<IdentityUserExtended> userManager,
            SignInManager<IdentityUserExtended> signInManager,
            UserAccountsManager userAccountsManager,
            AccountsManager accountsManager
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _userAccountsManager = userAccountsManager;
            _accountsManager = accountsManager;
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

            if (ExtendedUser.IsInRole("Administrator") && !wantedUser.IsBuiltInAdmin)
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

        [Route("/Identity/Account/Users/All")]
        [RoleAuthorize("Administrator")]
        [Owner]
        public async Task<IActionResult> Users()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var account = _userAccountsManager.GetUserAccount(currentUser);
            var users = _userManager.Users.ToList();

            return View(users.Select(x => new UserInfoModel
            {
                Id = x.Id,
                Name = x.UserName,
                Email = x.Email,
                Role = _userAccountsManager.GetUserRoleForAccount(x, account).Result.Name,
                IsCurrentUser = x == currentUser,
                IsBuiltInAdmin = x.IsBuiltInAdmin,
                IsOwner = _userAccountsManager.IsOwner(x, account)
            }));
        }

        [Route("/Identity/Account/Users/Assigned")]
        [Owner]
        public async Task<IActionResult> Assigned()
        {
            var users = _userManager.Users.ToList();
            var user = await _userManager.GetUserAsync(User);
            var currentUser = await _userManager.GetUserAsync(User);
            var account = _userAccountsManager.GetUserAccount(currentUser);
            var assignedUsers = users.Where(x => _userAccountsManager.BelongsTo(user, x));

            return View(assignedUsers.Select(x => new UserInfoModel
            {
                Id = x.Id,
                Name = x.UserName,
                Role = _userAccountsManager.GetUserRoleForAccount(x, account).Result.Name,
                IsCurrentUser = x == currentUser,
                IsBuiltInAdmin = x.IsBuiltInAdmin,
                IsOwner = _userAccountsManager.IsOwner(x, account)
            }));
        }

        [Owner]
        public async Task<IActionResult> Accounts(string id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var wantedUser = await _userManager.FindByIdAsync(id);

            if (!TryValidate(currentUser, id, wantedUser, out IActionResult result))
            {
                return result;
            }

            if (string.IsNullOrEmpty(id) || currentUser == wantedUser)
            {
                return View((_userAccountsManager.GetUserParentAccounts(currentUser), ""));
            }

            if (ExtendedUser.IsInRole("Administrator") && !wantedUser.IsBuiltInAdmin)
            {
                return View((_userAccountsManager.GetUserParentAccounts(wantedUser), wantedUser.Id));
            }

            return NotFound();
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
            var user = new IdentityUserExtended { UserName = registerModel.UserName, Email = registerModel.Email };
            var currentUser = await _userManager.GetUserAsync(User);
            var account = _userAccountsManager.GetUserAccount(currentUser);
            var results = new List<IdentityResult> { await _userManager.CreateAsync(user, registerModel.Password) };

            if (account.IsAppStoreAccount)
            {
                results.Add(await _userAccountsManager.TryAddUserToAccount(user.Id, registerModel.UserRole ?? "Developer", $"{user.UserName} Account", account.AccountName));
            }
            else
            {
                var appStoreAccount = _accountsManager.GetAppStoreAccount();
                results.Add(await _userAccountsManager.TryAddUserToAccount(user.Id, registerModel.UserRole ?? "Developer", $"{user.UserName} Account", appStoreAccount.AccountName));
                results.Add(await _userAccountsManager.TryAddUserToAccount(user.Id, registerModel.UserRole ?? "Developer", $"{user.UserName} Account", account.AccountName));
            }

            if (results.All(x => x.Succeeded))
            {
                TempData["StatusMessage"] = string.Format("Success! {0} was added!", user.UserName);

                return RedirectToAction("Assigned");
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
            _userAccountsManager.RemoveUserAccounts(user);
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
                return RedirectToAction("Assigned");
            }

            TempData["StatusMessage"] = string.Format("Error! {0}", result.Errors.First().Description);
            return RedirectToAction("Assigned");
        }

        [Authorize]
        [Owner]
        public async Task<IActionResult> Assign(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var account = _userAccountsManager.GetUserAccount(user);
            var entryAccount = _userAccountsManager.GetUserAccount(await _userManager.GetUserAsync(User));
            var result = await _userAccountsManager.TryAddUserToAccount(user.Id, roleName ?? "Developer", account.AccountName, entryAccount.AccountName);

            if (result.Succeeded)
            {
                TempData["StatusMessage"] = string.Format("Success! {0} was assigned succesfully", user.UserName);
                return Content("/Identity/Account/Users/Assigned");
            }

            TempData["StatusMessage"] = result.Errors.First().Description;
            return new EmptyResult();
        }

        [Authorize]
        [Owner]
        public async Task<IActionResult> CheckUserExistance(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return Json(new { Message = "The user does not exits!", IsErrorMessage = false });
            }

            return PartialView("_ExistentUserPartial", user.Id);
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
                IsUsernameEditable = !isCurrentUserProfile && user.IsBuiltInAdmin,
                Id = isCurrentUserProfile ? null : user.Id
            };
        }
    }
}

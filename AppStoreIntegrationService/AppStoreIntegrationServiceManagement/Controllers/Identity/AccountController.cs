using AppStoreIntegrationServiceManagement.Filters;
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
    public class AccountController : Controller
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

        [Authorize(Roles = "Administrator, Developer")]
        [Owner]
        public async Task<IActionResult> Users()
        {
            var user = await _userManager.GetUserAsync(User);
            var users = _userManager.Users.ToList();
            var identityUsers = User.IsInRole("Administrator") ? users : users.Where(x => _userAccountsManager.BelongsTo(user, x));

            return View(identityUsers.Select(x => new UserInfoModel
            {
                Id = x.Id,
                Name = x.UserName,
                Role = _userManager.GetRolesAsync(x).Result.FirstOrDefault(),
                IsCurrentUser = x == user,
                IsBuiltInAdmin = x.IsBuiltInAdmin
            }));
        }

        [Authorize(Roles = "Developer")]
        public async Task<IActionResult> Accounts()
        {
            var user = await _userManager.GetUserAsync(User);
            return View(_userAccountsManager.GetUserAccounts(user));
        }

        [Authorize(Roles = "Developer")]
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

        [Authorize(Roles = "Administrator, Developer")]
        [Owner]
        public async Task<IActionResult> Register()
        {
            return View(new RegisterModel
            {
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            });
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, Developer")]
        [Owner]
        public async Task<IActionResult> PostRegister(RegisterModel registerModel)
        {
            registerModel.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            var user = new IdentityUserExtended { UserName = registerModel.Input.UserName, Email = registerModel.Input.Email };
            var results = new List<IdentityResult>
            {
                await _userManager.CreateAsync(user, registerModel.Input.Password),
                await _userManager.AddToRoleAsync(user, registerModel.Input.UserRole)
            };

            if (User.IsInRole("Administrator"))
            {
                results.Add(await _userAccountsManager.TryAddUserToAccount(user));
            }

            if (User.IsInRole("Developer"))
            {
                var developer = await _userManager.GetUserAsync(User);
                var account = _accountsManager.GetAccountById(developer.SelectedAccountId);
                results.Add(await _userAccountsManager.TryAddUserToAccount(user, account.AccountName));
            }

            if (results.All(x => x.Succeeded))
            {
                TempData["StatusMessage"] = string.Format("Success! {0} was added!", user.UserName);
                return RedirectToAction("Users");
            }

            TempData["StatusMessage"] = string.Format("Error! {0}", results.FirstOrDefault(x => !x.Succeeded).Errors.First().Description);
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
                return Content(null);
            }

            await _userManager.DeleteAsync(user);
            _userAccountsManager.RemoveUserFromAccounts(user);
            TempData["StatusMessage"] = string.Format("Success! {0} was deleted!", user.UserName);
            return Content(null);
        }

        [Authorize(Roles = "Developer")]
        [Owner]
        public async Task<IActionResult> Dismiss(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            var currentUser = await _userManager.GetUserAsync(User);
            var account = _accountsManager.GetAccountById(currentUser.SelectedAccountId);
            var result = _userAccountsManager.RemoveUserFromAccount(user, account);

            if (result.Succeeded)
            {
                TempData["StatusMessage"] = string.Format("{0} was dismissed succesfully", user.UserName);
                return RedirectToAction("Users");
            }

            TempData["StatusMessage"] = string.Format("Error! {0}", result.Errors.First().Description);
            return RedirectToAction("Users");
        }

        [Authorize(Roles = "Developer")]
        [Owner]
        public async Task<IActionResult> Assign(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            var currentUser = await _userManager.GetUserAsync(User);
            var account = _accountsManager.GetAccountById(currentUser.SelectedAccountId);
            var result = await _userAccountsManager.TryAddUserToAccount(user, account.AccountName);

            if (result.Succeeded)
            {
                TempData["StatusMessage"] = string.Format("Success! {0} was assigned succesfully", user.UserName);
                return Content("/Identity/Account/Users");
            }

            TempData["StatusMessage"] = string.Format("Error! {0}", result.Errors.First().Description);
            return new EmptyResult();
        }

        [Authorize(Roles = "Developer")]
        [Owner]
        public async Task<IActionResult> CheckUserExistance(string username)
        {
            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
            {
                return Json(new { Message = "The user does not exits!", IsErrorMessage = false });
            }

            var roles = await _userManager.GetRolesAsync(user);

            if (roles.FirstOrDefault() != "Developer")
            {
                return Json(new { Message = "The user exists and cannot be assigned!", IsErrorMessage = true });
            }

            return PartialView("_ExistentUserPartial", user.Id);
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
            var roles = await _userManager.GetRolesAsync(user);

            return new ProfileModel
            {
                UserName = user.UserName,
                Email = user.Email,
                UserRole = roles.FirstOrDefault(),
                IsUsernameEditable = !isCurrentUserProfile || (!user.IsBuiltInAdmin && roles.FirstOrDefault() == "Administrator"),
                IsUserRoleEditable = !isCurrentUserProfile,
                Id = isCurrentUserProfile ? null : user.Id
            };
        }
    }
}

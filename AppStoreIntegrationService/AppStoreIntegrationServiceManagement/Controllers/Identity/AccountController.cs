using AppStoreIntegrationServiceCore.DataBase;
using AppStoreIntegrationServiceManagement.Filters;
using AppStoreIntegrationServiceManagement.Model;
using AppStoreIntegrationServiceManagement.Model.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace AppStoreIntegrationServiceManagement.Controllers.Identity
{
    [Area("Identity")]
    [Authorize]
    [AccountSelected]
    public class AccountController : CustomController
    {
        public async Task<IActionResult> Profile(string id)
        {
            var currentUser = await UserManager.GetUserAsync(User);
            var wantedUser = await UserManager.FindByIdAsync(id);

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
            var currentUser = await UserManager.GetUserAsync(User);
            var wantedUser = await UserManager.FindByIdAsync(id);
            List<IdentityResult> results = new();

            if (!TryValidate(currentUser, id, wantedUser, out IActionResult result))
            {
                return result;
            }

            if (string.IsNullOrEmpty(id) || currentUser == wantedUser)
            {
                results = new List<IdentityResult>
                {
                    await UserManager.SetUserNameAsync(currentUser, profileModel.UserName),
                    await UserManager.SetEmailAsync(currentUser, profileModel.UserName)
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
                await UserManager.SetUserNameAsync(wantedUser, profileModel.UserName),
                await UserManager.SetEmailAsync(wantedUser, profileModel.UserName)
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
            var currentUser = await UserManager.GetUserAsync(User);
            var wantedUser = await UserManager.FindByIdAsync(id);

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
            var currentUser = await UserManager.GetUserAsync(User);
            var wantedUser = await UserManager.FindByIdAsync(id);

            if (!TryValidate(currentUser, id, wantedUser, out IActionResult result))
            {
                return result;
            }

            if (string.IsNullOrEmpty(id) || currentUser == wantedUser)
            {
                var identityResult = await UserManager.ChangePasswordAsync(currentUser, model.Input.OldPassword, model.Input.NewPassword);
                if (identityResult.Succeeded)
                {
                    await SignInManager.RefreshSignInAsync(currentUser);
                    TempData["StatusMessage"] = "Success! Your password was changed!";
                }

                TempData["StatusMessage"] = string.Format("Error! {0}", identityResult.Errors.First().Description);
                return RedirectToAction("Profile");
            }

            var results = new List<IdentityResult>
            {
                await UserManager.RemovePasswordAsync(wantedUser),
                await UserManager.AddPasswordAsync(wantedUser, model.Input.NewPassword)
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
            var currentUser = await UserManager.GetUserAsync(User);
            var account = UserAccountsManager.GetUserAccount(currentUser);
            var users = UserManager.Users.ToList();

            return View(users.Select(x => new UserInfoModel
            {
                Id = x.Id,
                Name = x.UserName,
                Email = x.Email,
                Role = UserAccountsManager.GetUserRoleForAccount(x, account).Result.Name,
                IsCurrentUser = x == currentUser,
                IsBuiltInAdmin = x.IsBuiltInAdmin,
                IsOwner = UserAccountsManager.IsOwner(x, account)
            }));
        }

        [Route("/Identity/Account/Users/Assigned")]
        [Owner]
        public async Task<IActionResult> Assigned()
        {
            var users = UserManager.Users.ToList();
            var user = await UserManager.GetUserAsync(User);
            var currentUser = await UserManager.GetUserAsync(User);
            var account = UserAccountsManager.GetUserAccount(currentUser);
            var assignedUsers = users.Where(x => UserAccountsManager.BelongsTo(user, x));

            return View(assignedUsers.Select(x => new UserInfoModel
            {
                Id = x.Id,
                Name = x.UserName,
                Role = UserAccountsManager.GetUserRoleForAccount(x, account).Result.Name,
                IsCurrentUser = x == currentUser,
                IsBuiltInAdmin = x.IsBuiltInAdmin,
                IsOwner = UserAccountsManager.IsOwner(x, account)
            }));
        }

        [Owner]
        public async Task<IActionResult> Accounts(string id)
        {
            var currentUser = await UserManager.GetUserAsync(User);
            var wantedUser = await UserManager.FindByIdAsync(id);

            if (!TryValidate(currentUser, id, wantedUser, out IActionResult result))
            {
                return result;
            }

            if (string.IsNullOrEmpty(id) || currentUser == wantedUser)
            {
                return View((UserAccountsManager.GetUserParentAccounts(currentUser), ""));
            }

            if (ExtendedUser.IsInRole("Administrator") && !wantedUser.IsBuiltInAdmin)
            {
                return View((UserAccountsManager.GetUserParentAccounts(wantedUser), wantedUser.Id));
            }

            return NotFound();
        }

        [Owner]
        public async Task<IActionResult> UpdateAccount(Account account)
        {
            var user = await UserManager.GetUserAsync(User);

            if (!UserAccountsManager.IsOwner(user, account))
            {
                TempData["StatusMessage"] = "Error! You cannot edit this account!";
                return new EmptyResult();
            }

            AccountsManager.UpdateAccount(account);
            TempData["StatusMessage"] = "Success! The account was updated!";
            return new EmptyResult();
        }

        [Owner]
        public async Task<IActionResult> Register()
        {
            return View(new RegisterModel
            {
                ExternalLogins = (await SignInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            });
        }

        [HttpPost]
        [Owner]
        public async Task<IActionResult> PostRegister(RegisterModel registerModel)
        {
            var user = new IdentityUserExtended { UserName = registerModel.UserName, Email = registerModel.Email };
            var currentUser = await UserManager.GetUserAsync(User);
            var account = UserAccountsManager.GetUserAccount(currentUser);
            var results = new List<IdentityResult> { await UserManager.CreateAsync(user, registerModel.Password) };

            if (account.IsAppStoreAccount)
            {
                results.Add(await UserAccountsManager.TryAddUserToAccount(user.Id, registerModel.UserRole ?? "Developer", $"{user.UserName} Account", account.AccountName));
            }
            else
            {
                var appStoreAccount = AccountsManager.GetAppStoreAccount();
                results.Add(await UserAccountsManager.TryAddUserToAccount(user.Id, registerModel.UserRole ?? "Developer", $"{user.UserName} Account", appStoreAccount.AccountName));
                results.Add(await UserAccountsManager.TryAddUserToAccount(user.Id, registerModel.UserRole ?? "Developer", $"{user.UserName} Account", account.AccountName));
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
            var user = await UserManager.FindByIdAsync(id);
            var currentUser = await UserManager.GetUserAsync(User);

            if (!TryValidate(user, null, null, out IActionResult result))
            {
                return result;
            }

            if (user == currentUser)
            {
                TempData["StatusMessage"] = "Error! You cannot delete your account!";
                return Content(null);
            }

            await UserManager.DeleteAsync(user);
            UserAccountsManager.RemoveUserAccounts(user);
            TempData["StatusMessage"] = string.Format("Success! {0} was deleted!", user.UserName);
            return Content(null);
        }

        [Authorize]
        [Owner]
        public async Task<IActionResult> Dismiss(string id)
        {
            var user = await UserManager.FindByIdAsync(id);
            var currentUser = await UserManager.GetUserAsync(User);
            var account = UserAccountsManager.GetUserAccount(currentUser);
            var result = UserAccountsManager.RemoveUserFromAccount(user, account);

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
            var user = await UserManager.FindByIdAsync(userId);
            var account = UserAccountsManager.GetUserAccount(user);
            var entryAccount = UserAccountsManager.GetUserAccount(await UserManager.GetUserAsync(User));
            var result = await UserAccountsManager.TryAddUserToAccount(user.Id, roleName ?? "Developer", account.AccountName, entryAccount.AccountName);

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
            var user = await UserManager.FindByEmailAsync(email);

            if (user == null)
            {
                return Json(new { Message = "The user does not exits!", IsErrorMessage = false });
            }

            return PartialView("_ExistentUserPartial", user.Id);
        }

        [Authorize]
        public IActionResult AccessToken()
        {
            return View("AccessToken");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> GenerateAccessToken()
        {
            var descriptor = new SecurityTokenDescriptor();
            var tokenHandler = new JwtSecurityTokenHandler();
            var user = await UserManager.GetUserAsync(User);

            var securityToken = tokenHandler.CreateToken(descriptor);
            var token = tokenHandler.WriteToken(securityToken);
            user.APIAccessToken = token;
            await UserManager.UpdateAsync(user);
            return PartialView("_AccessTokenPartial", token);
        }

        private bool TryValidate(IdentityUserExtended currentUser, string id, IdentityUserExtended wantedUser, out IActionResult result)
        {
            if (currentUser == null)
            {
                result = NotFound($"Unable to load user with ID '{UserManager.GetUserId(User)}'.");
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

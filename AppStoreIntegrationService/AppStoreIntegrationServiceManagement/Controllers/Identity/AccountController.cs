using AppStoreIntegrationServiceCore.DataBase;
using AppStoreIntegrationServiceCore.DataBase.Models;
using AppStoreIntegrationServiceManagement.Filters;
using AppStoreIntegrationServiceManagement.Model;
using AppStoreIntegrationServiceManagement.Model.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AppStoreIntegrationServiceManagement.ExtensionMethods;
using AppStoreIntegrationServiceManagement.Repository.Interface;
using AppStoreIntegrationServiceManagement.Model.Notifications;

namespace AppStoreIntegrationServiceManagement.Controllers.Identity
{
    [Area("Identity")]
    [Authorize]
    [DBSynched]
    [AccountSelect]
    [TechPartnerAgreement]
    public class AccountController : CustomController
    {
        private readonly INotificationCenter _notificationCenter;

        public AccountController(INotificationCenter notificationCenter)
        {
            _notificationCenter = notificationCenter;
        }

        public IActionResult Profile(string id)
        {
            var currentUser = UserManager.GetUser(User);
            var wantedUser = UserManager.GetUserById(id);

            if (!TryValidate(currentUser, id, wantedUser, out IActionResult result))
            {
                return result;
            }

            if (string.IsNullOrEmpty(id) || currentUser.Equals(wantedUser))
            {
                currentUser.Id = null;
                return View(currentUser);
            }

            if (ExtendedUser.IsInRole("SystemAdministrator"))
            {
                return View(wantedUser);
            }

            return NotFound();
        }

        [Route("/Identity/Account/Users/All")]
        [RoleAuthorize("SystemAdministrator")]
        public IActionResult Users()
        {
            var currentUser = UserManager.GetUser(User);
            var users = UserManager.UserProfiles.Where(x => !string.IsNullOrEmpty(x.UserId));

            return View(users.Select(x => new UserInfoModel
            {
                Id = x.Id,
                Name = x.Name.ToUpperFirst(),
                Email = x.Email,
                IsCurrentUser = x.Id == currentUser.Id,
                IsBuiltInAdmin = x.Name == "admin",
                IsEligibleForRemoval = UserAccountsManager.CanBeRemoved(x)
            }));
        }

        [Route("/Identity/Account/Users/Assigned")]
        [RoleAuthorize("SystemAdministrator", "Administrator")]
        public IActionResult Assigned()
        {
            var users = UserManager.UserProfiles;
            var user = UserManager.GetUser(User);
            var account = AccountsManager.GetAccountById(user.SelectedAccountId);
            var assignedUsers = users.Where(x => UserAccountsManager.BelongsTo(x, account) && !string.IsNullOrEmpty(x.UserId));

            return View(assignedUsers.Select(x => new UserInfoModel
            {
                Id = x.Id,
                Name = x.Name.ToUpperFirst(),
                Role = UserAccountsManager.GetUserRoleForAccount(x, account).Name,
                IsCurrentUser = x.Email == user.Email
            }));
        }

        public IActionResult Accounts(string id)
        {
            var currentUser = UserManager.GetUser(User);
            var wantedUser = UserManager.GetUserById(id);

            if (!TryValidate(currentUser, id, wantedUser, out IActionResult result))
            {
                return result;
            }

            if (string.IsNullOrEmpty(id) || currentUser == wantedUser)
            {
                return View((PrepareAccounts(currentUser), ""));
            }

            if (ExtendedUser.IsInRole("SystemAdministrator") && wantedUser.Name != "admin")
            {
                return View((PrepareAccounts(wantedUser), wantedUser.Id));
            }

            return NotFound();
        }

        [RoleAuthorize("SystemAdministrator", "Administrator")]
        public IActionResult Register()
        {
            return View(new RegisterModel());
        }

        [HttpPost]
        [RoleAuthorize("SystemAdministrator", "Administrator")]
        public async Task<IActionResult> PostRegister(RegisterModel registerModel)
        {
            var currentUser = UserManager.GetUser(User);
            var user = new UserProfile { Email = registerModel.Email, Id = Guid.NewGuid().ToString() };
            var createNew = ExtendedUser.IsInRole("SystemAdministrator") && registerModel.UserRole == "Administrator";
            var account = createNew ? AccountsManager.TryAddAccount(new Account
            {
                OosId = registerModel.OosId,
                Id = Guid.NewGuid().ToString(),
                SalesForceId = registerModel.SalesForceId,
                Name = registerModel.SalesForceName

            }) : AccountsManager.GetAccountById(currentUser?.SelectedAccountId);

            UserManager.AddUserProfile(user);
            UserAccountsManager.TryAddUserToAccount(new UserAccount
            {
                AccountId = account.Id,
                UserProfileId = user.Id,
                Id = Guid.NewGuid().ToString(),
                UserRoleId = RoleManager.GetRoleByName(registerModel.UserRole).Id,
            });

            await Notify(createNew, registerModel.Email, account.Name);
            TempData["StatusMessage"] = string.Format("Success! {0} was added!", user.Email);
            return Content("/Identity/Account/Users/Assigned");
        }

        [RoleAuthorize("SystemAdministrator")]
        public IActionResult Delete(string id)
        {
            var user = UserManager.GetUserById(id);

            if (!TryValidate(user, null, null, out IActionResult result))
            {
                return result;
            }

            UserManager.Delete(user);
            UserAccountsManager.RemoveUserAccounts(user);
            AccountAgreementsManager.Remove(user);
            TempData["StatusMessage"] = string.Format("Success! {0} was deleted!", user.Name);
            return new EmptyResult();
        }

        [RoleAuthorize("SystemAdministrator", "Administrator")]
        public IActionResult Dismiss(string id)
        {
            var user = UserManager.GetUserById(id);
            var currentUser = UserManager.GetUser(User);
            var account = AccountsManager.GetAccountById(currentUser.SelectedAccountId);
            var result = UserAccountsManager.RemoveUserFromAccount(user, account);
            AccountAgreementsManager.Remove(user, account);

            if (result.Succeeded)
            {
                TempData["StatusMessage"] = string.Format("Success! {0} was dismissed succesfully", user.Name.ToUpperFirst());
                return Content("/Identity/Account/Assigned");
            }

            TempData["StatusMessage"] = string.Format("Error! {0}", result.Errors.First().Description);
            return Content("/Identity/Account/Assigned");
        }

        [RoleAuthorize("SystemAdministrator", "Administrator")]
        public async Task<IActionResult> Assign(string userId, string roleName)
        {
            var user = UserManager.GetUserById(userId);
            var currentUser = UserManager.GetUser(User);
            var account = AccountsManager.GetAccountById(currentUser.SelectedAccountId);
            var roleId = RoleManager.GetRoleByName(roleName).Id;

            var result = UserAccountsManager.TryAddUserToAccount(new UserAccount
            {
                UserProfileId = userId,
                AccountId = account.Id,
                UserRoleId = roleId,
                Id = Guid.NewGuid().ToString()
            });

            AccountAgreementsManager.Add(new AccountAgreement
            {
                Id = Guid.NewGuid().ToString(),
                UserProfileId = user.Id,
                AccountId = account.Id
            });

            await Notify(false, user.Email, account.Name);

            if (result.Succeeded)
            {
                TempData["StatusMessage"] = string.Format("Success! {0} was assigned succesfully", user.Name);
                return Content("/Identity/Account/Users/Assigned");
            }

            return PartialView("_StatusMessage", string.Format("{0}", result.Errors.First().Description));
        }

        [RoleAuthorize("SystemAdministrator", "Administrator")]
        public IActionResult CheckUserExistance(string email)
        {
            var user = UserManager.GetUserByEmail(email);

            if (user == null)
            {
                return Json(new { Message = "The user does not exits!", IsErrorMessage = false });
            }

            var currentUserEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            if (email == currentUserEmail)
            {
                return Json(new { Message = "This user cannot be assigned!", IsErrorMessage = true });
            }

            return PartialView("_ExistentUserPartial", user.Id);
        }

        public IActionResult AccessToken()
        {
            return View("AccessToken");
        }

        [HttpPost]
        public IActionResult GenerateAccessToken()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var user = UserManager.GetUser(User);

            var securityToken = tokenHandler.CreateToken(new());
            var token = tokenHandler.WriteToken(securityToken);
            user.APIAccessToken = token;
            UserManager.UpdateUserProfile(user);
            return PartialView("_AccessTokenPartial", token);
        }

        private async Task Notify(bool createNew, string email, string account)
        {
            if (createNew)
            {
                await _notificationCenter.SendEmail(new EmailNotification
                {
                    Author = email,
                    Message = "A new user profile was created on RWS AppStore Manager using this email address:",
                    Title = account,
                    CallToActionUrl = GetUrlBase() + "/Plugins",
                    IsAccountNotification = true
                });

                return;
            }

            await _notificationCenter.SendEmail(new EmailNotification
            {
                Author = email,
                Message = "A user profile identified by this email address was associated to a new account in RWS AppStore Manager:",
                Title = account,
                CallToActionUrl = GetUrlBase() + "/Plugins",
                IsAccountNotification = true
            });
        }

        private IEnumerable<AccountModel> PrepareAccounts(UserProfile user)
        {
            var accounts = UserAccountsManager.GetUserAccounts(user);
            return accounts.Select(x => new AccountModel
            {
                Name = x.Name,
                Role = UserAccountsManager.GetUserRoleForAccount(user, x).Name,
            });
        }

        private bool TryValidate(UserProfile currentUser, string id, UserProfile wantedUser, out IActionResult result)
        {
            if (currentUser == null)
            {
                result = NotFound($"Unable to load user with ID '{UserProfilesManager.GetUserId(User)}'.");
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
    }
}

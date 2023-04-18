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
using AppStoreIntegrationServiceManagement.DataBase.Interface;
using AppStoreIntegrationServiceCore.DataBase.Interface;
using AppStoreIntegrationServiceCore.DataBase;
using AppStoreIntegrationServiceCore.DataBase.Models;
using Microsoft.AspNetCore.Identity;

namespace AppStoreIntegrationServiceManagement.Controllers.Identity
{
    [Area("Identity")]
    [Authorize]
    [DBSynched]
    [AccountSelect]
    [TechPartnerAgreement]
    public class AccountController : CustomController
    {
        private readonly IAccountAgreementsManager _accountAgreementsManager;
        private readonly IUserProfilesManager _userProfilesManager;
        private readonly IUserAccountsManager _userAccountsManager;
        private readonly INotificationCenter _notificationCenter;
        private readonly IUserRolesManager _userRolesManager;
        private readonly IAccountsManager _accountsManager;
        private readonly IAuth0UserManager _auth0UserManager;

        public AccountController
        (
            IAccountAgreementsManager accountAgreementsManager,
            IUserProfilesManager userProfilesManager,
            IUserAccountsManager userAccountsManager,
            INotificationCenter notificationCenter,
            IAccountsManager accountsManager,
            IUserRolesManager userRolesManager,
            IAuth0UserManager auth0UserManager
        ) : base(userProfilesManager, userAccountsManager, accountsManager)
        {
            _accountAgreementsManager = accountAgreementsManager;
            _notificationCenter = notificationCenter;
            _userProfilesManager = userProfilesManager;
            _userAccountsManager = userAccountsManager;
            _userRolesManager = userRolesManager;
            _accountsManager = accountsManager;
            _auth0UserManager = auth0UserManager;
        }

        public IActionResult Profile(string id)
        {
            var currentUser = _userProfilesManager.GetUser(User);
            var wantedUser = _userProfilesManager.GetUserById(id);

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

        [HttpPost]
        public async Task<IActionResult> ChangeRole(string role, string id)
        {
            var user = _userProfilesManager.GetUserById(id);
            var currentUser = _userProfilesManager.GetUser(User);
            var account = _accountsManager.GetAccountById(currentUser.SelectedAccountId);
            var result = await _userAccountsManager.ChangeUserRoleForAccount(user, account, role);

            if (result.Succeeded)
            {
                TempData["StatusMessage"] = $"Success! The role for {user.Name.ToUpperFirst()} was changed!";
                return new EmptyResult();
            }

            return PartialView("_StatusMessage", $"Error! {result.Errors.First().Description}");
        }

        [Route("/Identity/Account/Users/All")]
        [RoleAuthorize("SystemAdministrator")]
        public IActionResult Users()
        {
            var user = _userProfilesManager.GetUser(User);
            var users = _userProfilesManager.UserProfiles;
            var extendedProfiles = users.Where(x => !string.IsNullOrEmpty(x?.UserId) && _userAccountsManager.BelongsTo(x))
                                        .Select(x => new ExtendedUserProfile(x));

            return View(new UsersModel
            {
                ExtendedUsers = extendedProfiles,
                UnconfirmedUsers = _userProfilesManager.UserProfiles.Count(x => string.IsNullOrEmpty(x?.UserId))
            });
        }

        [Route("/Identity/Account/Users/Assigned")]
        public IActionResult Assigned()
        {
            var users = _userProfilesManager.UserProfiles;
            var user = _userProfilesManager.GetUser(User);
            var account = _accountsManager.GetAccountById(user.SelectedAccountId);
            var assignedUsers = users.Where(x => _userAccountsManager.BelongsTo(x, account) && !string.IsNullOrEmpty(x?.UserId));

            return View(new UsersModel
            {
                ExtendedUsers = assignedUsers.Select(x =>
                new ExtendedUserProfile(_userAccountsManager, user, account, x)
                {
                    Id = x.Id,
                    Name = x.Name.ToUpperFirst(),
                }),
                UnconfirmedUsers = users.Count(x => _userAccountsManager.BelongsTo(x, account) && string.IsNullOrEmpty(x?.UserId))
            });
        }

        public IActionResult Accounts(string id)
        {
            var currentUser = _userProfilesManager.GetUser(User);
            var wantedUser = _userProfilesManager.GetUserById(id);

            if (!TryValidate(currentUser, id, wantedUser, out IActionResult result))
            {
                return result;
            }

            if (string.IsNullOrEmpty(id) || currentUser == wantedUser)
            {
                return View((PrepareAccounts(currentUser), ""));
            }

            if (ExtendedUser.IsInRole("SystemAdministrator") && !wantedUser.IsBuiltInAdmin())
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
            var currentUser = _userProfilesManager.GetUser(User);
            var user = new UserProfile { Email = registerModel.Email, Id = Guid.NewGuid().ToString() };
            var createNew = ExtendedUser.IsInRole("SystemAdministrator") && registerModel.UserRole == "Administrator";
            var auth0User = await _auth0UserManager.GetUserByEmail(user?.Email);
            var results = new List<IdentityResult>();
            var account = createNew ? await _accountsManager.TryAddAccount(new Account
            {
                OosId = registerModel.OosId,
                Id = Guid.NewGuid().ToString(),
                SalesForceId = registerModel.SalesForceId,
                Name = string.IsNullOrEmpty(auth0User?.Name) ? registerModel.SalesForceName : $"{auth0User.Name.ToUpperFirst()} Account"
            }) : _accountsManager.GetAccountById(currentUser?.SelectedAccountId);

            results.Add(await _userProfilesManager.TryAddUserProfile(user));
            results.Add(await _userAccountsManager.TryAddUserToAccount(new UserAccount
            {
                AccountId = account.Id,
                UserProfileId = user.Id,
                Id = Guid.NewGuid().ToString(),
                UserRoleId = _userRolesManager.GetRoleByName(registerModel.UserRole).Id,
            }));
            results.Add(await Notify(createNew, registerModel.Email, account.Name));

            if (results.Any(x => !x.Succeeded))
            {
                return PartialView("_StatusMessage", string.Format("Error! {0}!", results.First(x => !x.Succeeded).Errors.First().Description));
            }

            TempData["StatusMessage"] = string.Format("Success! {0} was added!", user.Email);
            return Content("/Identity/Account/Users/Assigned");
        }

        public async Task<IActionResult> Dismiss(string userId, string accountId = null)
        {
            var user = _userProfilesManager.GetUserById(userId);
            var currentUser = _userProfilesManager.GetUser(User);
            var account = _accountsManager.GetAccountById(accountId ?? currentUser.SelectedAccountId);
            var result = await _userAccountsManager.RemoveUserFromAccount(user, account);
            await _accountAgreementsManager.Remove(user, account);

            if (result.Succeeded)
            {
                if (user.Id == currentUser.Id)
                {
                    user.SelectedAccountId = null;
                    await _userProfilesManager.TryUpdateUserProfile(user);
                    TempData["StatusMessage"] = string.Format("Success! You left the {0}!", account.Name);
                    return Content("/Identity/Authentication/Accounts");
                }

                TempData["StatusMessage"] = string.Format("Success! {0} was dismissed succesfully", user.Name.ToUpperFirst());
                return Content("/Identity/Account/Users/Assigned");
            }

            TempData["StatusMessage"] = string.Format("Error! {0}", result.Errors.First().Description);
            return Content("/Identity/Account/Users/Assigned");
        }

        [RoleAuthorize("SystemAdministrator", "Administrator")]
        public async Task<IActionResult> Assign(AssignModel model)
        {
            var user = _userProfilesManager.GetUserById(model.UserId);
            var currentUser = _userProfilesManager.GetUser(User);
            var account = _accountsManager.GetAccountById(currentUser.SelectedAccountId);
            var role = _userRolesManager.GetRoleByName(model.UserRole);

            var result = await _userAccountsManager.TryAddUserToAccount(new UserAccount
            {
                UserProfileId = model.UserId,
                AccountId = account.Id,
                UserRoleId = role.Id,
                Id = Guid.NewGuid().ToString()
            });

            await Notify(false, user.Email, account.Name);

            if (result.Succeeded)
            {
                TempData["StatusMessage"] = string.Format("Success! {0} was assigned succesfully!", user.Name.ToUpperFirst());
                return Content("/Identity/Account/Users/Assigned");
            }

            return PartialView("_StatusMessage", string.Format("{0}", result.Errors.First().Description));
        }

        [RoleAuthorize("SystemAdministrator", "Administrator")]
        public IActionResult CheckUserExistance(string email)
        {
            var user = _userProfilesManager.GetUserByEmail(email);

            if (user == null)
            {
                return Json(new { Message = "The user does not exist!", IsErrorMessage = false });
            }

            var currentUserEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            if (email == currentUserEmail)
            {
                return Json(new { Message = "This user cannot be assigned!", IsErrorMessage = true });
            }

            return PartialView("_ExistentUserPartial", new AssignModel { UserId = user.Id });
        }

        public IActionResult AccessToken()
        {
            return View("AccessToken");
        }

        [HttpPost]
        public async Task<IActionResult> GenerateAccessToken()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var user = _userProfilesManager.GetUser(User);

            var securityToken = tokenHandler.CreateToken(new ());
            var token = tokenHandler.WriteToken(securityToken);
            user.APIAccessToken = token;
            var result = await _userProfilesManager.TryUpdateUserProfile(user);

            if (result.Succeeded)
            {
                return PartialView("_AccessTokenPartial", token);
            }

            TempData["StatusMessage"] = string.Format("Error! {0}!", result.Errors.First().Description);
            return Content("/Identity/Account/AccessToken");
        }

        private async Task<IdentityResult> Notify(bool createNew, string email, string account)
        {
            if (createNew)
            {
                return await _notificationCenter.SendEmail(new EmailNotification
                {
                    Author = email,
                    Message = "A new user profile was created on RWS AppStore Manager using this email address:",
                    Title = account,
                    CallToActionUrl = GetUrlBase() + "/Plugins",
                    IsAccountNotification = true
                });
            }

            return await _notificationCenter.SendEmail(new EmailNotification
            {
                Author = email,
                Message = "A user profile identified by this email address was associated to a new account in RWS AppStore Manager:",
                Title = account,
                CallToActionUrl = GetUrlBase() + "/Plugins",
                IsAccountNotification = true
            });
        }

        private IEnumerable<ExtendedAccount> PrepareAccounts(UserProfile user)
        {
            var accounts = _userAccountsManager.GetUserAccounts(user);
            return accounts.Select(x => new ExtendedAccount(_userAccountsManager, user, x));
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

using AppStoreIntegrationServiceCore.DataBase.Interface;
using AppStoreIntegrationServiceCore.DataBase.Models;
using AppStoreIntegrationServiceManagement.DataBase.Interface;
using Microsoft.AspNetCore.Identity;
using System.Data;

namespace AppStoreIntegrationServiceManagement.DataBase
{
    public class UserAccountsManager : IUserAccountsManager
    {
        private readonly IAccountEntitlementsManager _accountEntitlementsManager;
        private readonly IServiceContextFactory _serviceContext;
        private readonly IAccountsManager _accountsManager;
        private readonly IUserRolesManager _roleManager;
        private readonly IUserProfilesManager _userProfilesManager;

        public UserAccountsManager
        (
            IAccountEntitlementsManager accountEntitlementsManager,
            IServiceContextFactory serviceContext,
            IAccountsManager accountsManager,
            IUserRolesManager roleManager,
            IUserProfilesManager userProfilesManager
        )
        {
            _accountEntitlementsManager = accountEntitlementsManager;
            _serviceContext = serviceContext;
            _accountsManager = accountsManager;
            _roleManager = roleManager;
            _userProfilesManager = userProfilesManager;
        }

        public IEnumerable<Account> GetUserAccounts(UserProfile user)
        {
            using (var context = _serviceContext.CreateContext())
            {
                var userAccounts = context.UserAccounts.ToList();
                return userAccounts.Where(x => x.UserProfileId == user.Id)
                                   .Select(x => _accountsManager.GetAccountById(x.AccountId));
            }
        }

        public Account GetUserUnsyncedAccount(UserProfile user)
        {
            if (string.IsNullOrEmpty(user?.Id))
            {
                return null;
            }

            using (var context = _serviceContext.CreateContext())
            {
                var userAccounts = context.UserAccounts.ToList();

                foreach (var userAccount in userAccounts.Where(x => x.UserProfileId == user.Id))
                {
                    var account = _accountsManager.GetAccountById(userAccount.AccountId);

                    if (userAccounts.Count(x => x.AccountId == userAccount.AccountId) == 1 && string.IsNullOrEmpty(account.Name))
                    {
                        return account;
                    }
                }

                return null;
            }
        }

        public bool CanBeRemoved(UserProfile user)
        {
            if (string.IsNullOrEmpty(user?.Id))
            {
                return false;
            }

            using (var context = _serviceContext.CreateContext())
            {
                var userAccounts = context.UserAccounts.ToList();
                foreach (var userAccount in userAccounts.Where(x => x.UserProfileId == user.Id))
                {
                    var otherAccounts = userAccounts.Where(x => x.UserProfileId != user.Id && x.AccountId == userAccount.AccountId);
                    if (otherAccounts.Any(x => CanBeRemoved(_roleManager.GetRoleById(x.UserRoleId))))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public async Task<IdentityResult> RemoveUserFromAllAccounts(UserProfile user)
        {
            try
            {
                using (var context = _serviceContext.CreateContext())
                {
                    var userAccounts = context.UserAccounts;
                    var toBeRemoved = userAccounts.Where(x => x.UserProfileId == user.Id);
                    userAccounts.RemoveRange(toBeRemoved);
                    await context.SaveChangesAsync();
                    return IdentityResult.Success;
                }
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = ex.Message });
            }
        }

        public bool BelongsTo(UserProfile member, Account account)
        {
            try
            {
                using (var context = _serviceContext.CreateContext())
                {
                    return context.UserAccounts.ToList().Any(x => x.AccountId == account.Id && x.UserProfileId == member.Id);
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public UserRole GetUserRoleForAccount(UserProfile user, Account account)
        {
            try
            {
                using (var context = _serviceContext.CreateContext())
                {
                    var userAccounts = context.UserAccounts.ToList();
                    var userAccount = userAccounts.FirstOrDefault(x => x.UserProfileId == user.Id && x.AccountId == account.Id);
                    return _roleManager.GetRoleById(userAccount.UserRoleId);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<IdentityResult> RemoveUserFromAccount(UserProfile user, Account account)
        {
            try
            {
                using (var context = _serviceContext.CreateContext())
                {
                    var userAccounts = context.UserAccounts;
                    var userAccount = userAccounts.FirstOrDefault(x => x.UserProfileId == user.Id && x.AccountId == account.Id);
                    userAccounts.Remove(userAccount);
                    await context.SaveChangesAsync();
                    return IdentityResult.Success;
                }
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = ex.Message });
            }
        }

        public async Task<IdentityResult> TryAddUserToAccount(UserAccount userAccount)
        {
            try
            {
                using (var context = _serviceContext.CreateContext())
                {
                    var userAccounts = context.UserAccounts;

                    if (userAccounts.ToList().Any(x => x.IsAssigned(userAccount)))
                    {
                        return IdentityResult.Failed(new IdentityError 
                        { 
                            Description = "Warning! The user is already assigned to this account" 
                        });
                    }

                    userAccounts.Add(userAccount);
                    await context.SaveChangesAsync();
                    await _accountEntitlementsManager.Add(new AccountEntitlement
                    {
                        Id = Guid.NewGuid().ToString(),
                        AccountId = userAccount.AccountId
                    });

                    return IdentityResult.Success;
                }
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = ex.Message });
            }
        }

        public IEnumerable<UserProfile> GetUsersFromAccount(Account account)
        {
            if (string.IsNullOrEmpty(account?.Id))
            {
                return new List<UserProfile>();
            }

            using (var context = _serviceContext.CreateContext())
            {
                var userAccounts = context.UserAccounts.ToList();
                return userAccounts.Where(x => x.AccountId == account.Id)
                                   .Select(x => _userProfilesManager.GetUserById(x.UserProfileId));

            }
        }

        private static bool CanBeRemoved(UserRole role)
        {
            return "SystemAdministrator" == role.Name || "Administrator" == role.Name;
        }
    }
}

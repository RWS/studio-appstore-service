using AppStoreIntegrationServiceCore.DataBase.Interface;
using AppStoreIntegrationServiceCore.DataBase.Models;
using Microsoft.AspNetCore.Identity;
using System.Data;

namespace AppStoreIntegrationServiceCore.DataBase
{
    public class UserAccountsManager : IUserAccountsManager
    {
        private readonly IAccountEntitlementsManager _accountEntitlementsManager;
        private readonly IServiceContextFactory _serviceContext;
        private readonly IAccountsManager _accountsManager;
        private readonly IUserRolesManager _roleManager;

        public UserAccountsManager
        (
            IAccountEntitlementsManager accountEntitlementsManager,
            IServiceContextFactory serviceContext,
            IAccountsManager accountsManager,
            IUserRolesManager roleManager
        )
        {
            _accountEntitlementsManager = accountEntitlementsManager;
            _serviceContext = serviceContext;
            _accountsManager = accountsManager;
            _roleManager = roleManager;
        }

        public IEnumerable<Account> GetUserAccounts(UserProfile user)
        {
            using var context = _serviceContext.CreateContext();
            var userAccounts = context.UserAccounts.ToList();
            return userAccounts.Where(x => x.UserProfileId == user.Id)
                               .Select(x => _accountsManager.GetAccountById(x.AccountId));
        }

        public bool CanBeRemoved(UserProfile user)
        {
            using var context = _serviceContext.CreateContext();
            var userAccounts = context.UserAccounts.ToList();
            foreach (var userAccount in userAccounts.Where(x => x.UserProfileId == user.Id))
            {
                var otherAccounts = userAccounts.Where(x => x.UserProfileId != user.Id && x.AccountId == userAccount.Id);
                if (otherAccounts.Any(x => CanBeRemoved(_roleManager.GetRoleById(x.UserRoleId))))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool CanBeRemoved(UserRole role)
        {
            return "SystemAdministrator" == role.Name || "Administrator" == role.Name;
        }

        public IdentityResult RemoveUserAccounts(UserProfile user)
        {
            try
            {
                using var context = _serviceContext.CreateContext();
                var userAccounts = context.UserAccounts;
                var toBeRemobed = userAccounts.Where(x => x.UserProfileId == user.Id);
                userAccounts.RemoveRange(toBeRemobed);
                context.SaveChanges();
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = ex.Message });
            }
        }

        public bool BelongsTo(UserProfile member, Account account)
        {
            using var context = _serviceContext.CreateContext();
            return context.UserAccounts.ToList().Any(x => x.AccountId == account.Id && x.UserProfileId == member.Id);
        }

        public UserRole GetUserRoleForAccount(UserProfile user, Account account)
        {
            using var context = _serviceContext.CreateContext();
            var userAccounts = context.UserAccounts.ToList();
            var userAccount = userAccounts.FirstOrDefault(x => x.UserProfileId == user.Id && x.AccountId == account.Id);
            return _roleManager.GetRoleById(userAccount.UserRoleId);
        }

        public IdentityResult RemoveUserFromAccount(UserProfile user, Account account)
        {
            try
            {
                using var context = _serviceContext.CreateContext();
                var userAccounts = context.UserAccounts;
                var userAccount = userAccounts.FirstOrDefault(x => x.UserProfileId == user.Id && x.AccountId == account.Id);
                userAccounts.Remove(userAccount);
                context.SaveChanges();
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = ex.Message });
            }
        }

        public IdentityResult TryAddUserToAccount(UserAccount userAccount)
        {
            try
            {
                using var context = _serviceContext.CreateContext();
                var userAccounts = context.UserAccounts;

                if (userAccounts.ToList().Any(x => x.IsAssigned(userAccount)))
                {
                    return IdentityResult.Failed(new IdentityError { Description = "Warning! The user is already assigned to this account" });
                }

                userAccounts.Add(userAccount);
                context.SaveChanges();
                _accountEntitlementsManager.Add(new AccountEntitlement
                {
                    Id = Guid.NewGuid().ToString(),
                    AccountId = userAccount.AccountId
                });

                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = ex.Message });
            }
        }
    }
}

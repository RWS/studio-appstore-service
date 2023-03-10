using AppStoreIntegrationServiceManagement.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using System.Data;

namespace AppStoreIntegrationServiceManagement.Model.DataBase
{
    public class UserAccountsManager
    {
        private readonly AppStoreIntegrationServiceContext _context;
        private readonly AccountsManager _accountsManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserAccountsManager
        (
            AppStoreIntegrationServiceContext context,
            AccountsManager accountsManager,
            RoleManager<IdentityRole> roleManager
        )
        {
            _context = context;
            _accountsManager = accountsManager;
            _roleManager = roleManager;
        }

        public IEnumerable<Account> GetUserParentAccounts(IdentityUserExtended user)
        {
            var userAccounts = _context.UserAccounts.ToList();
            var currentUserAccounts = userAccounts.Where(x => x.UserId == user.Id);

            foreach (var userAccount in userAccounts.Where(x => x.UserId == user.Id))
            {
                yield return _accountsManager.GetAccountById(userAccount.ParentAccountId);
            }
        }

        public Account GetUserAccount(IdentityUserExtended user)
        {
            var userAccounts = _context.UserAccounts.ToList();
            var userAccount = userAccounts.FirstOrDefault(x => x.UserId == user.Id);
            return _accountsManager.GetAccountById(userAccount.AccountId);
        }

        public IdentityResult RemoveUserAccounts(IdentityUserExtended user)
        {
            try
            {
                var userAccounts = _context.UserAccounts;

                foreach (var userAccount in userAccounts.Where(x => x.UserId == user.Id))
                {
                    if (userAccount.IsOwner)
                    {
                        _accountsManager.RemoveAccountById(userAccount.AccountId);
                        userAccounts.RemoveRange(userAccounts.Where(x => x.AccountId == userAccount.AccountId));
                    }
                    else
                    {
                        userAccounts.Remove(userAccount);
                    }
                }

                _context.SaveChanges();
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = ex.Message });
            }
        }

        public bool IsOwner(IdentityUserExtended user)
        {
            var userAccounts = _context.UserAccounts.ToList();
            var account = _accountsManager.GetAccountById(user.SelectedAccountId);
            var userAccount = userAccounts.FirstOrDefault(x => x.UserId == user.Id && (x.AccountId == account.Id || x.ParentAccountId == account.Id));
            return userAccount.IsOwner;
        }

        public bool IsOwner(IdentityUserExtended user, Account account)
        {
            var userAccounts = _context.UserAccounts.ToList();
            var userAccount = userAccounts.FirstOrDefault(x => x.UserId == user.Id && x.AccountId == account.Id);
            return userAccount?.IsOwner ?? false;
        }

        public bool BelongsTo(IdentityUserExtended owner, IdentityUserExtended member)
        {
            var account = GetUserAccount(owner);
            var userAccounts = _context.UserAccounts.ToList();
            return userAccounts.Any(x => (x.ParentAccountId == account.Id || x.AccountId == account.Id) && x.UserId == member.Id);
        }

        public bool BelongsTo(IdentityUserExtended user, string accountId)
        {
            var userAccounts = _context.UserAccounts.ToList();
            return userAccounts.Any(x => x.ParentAccountId == accountId && x.UserId == user.Id);
        }

        public bool IsInRole(IdentityUserExtended user, string roleId)
        {
            var userAccounts = _context.UserAccounts.ToList();
            return userAccounts.Any(x => x.IsInRole(user, roleId));
        }

        public bool HasFullOwnership(IdentityUserExtended user, string roleId)
        {
            var userAccounts = _context.UserAccounts.ToList();
            return userAccounts.Any(x => x.HasFullOwnership(user, roleId));
        }

        public bool HasAssociatedAccounts(IdentityUserExtended user)
        {
            var userAccounts = _context.UserAccounts.ToList();
            return userAccounts.Any(x => x.UserId == user.Id);
        }

        public async Task<IdentityRole> GetUserRoleForAccount(IdentityUserExtended user, Account account)
        {
            var userAccounts = _context.UserAccounts.ToList();
            var userAccount = userAccounts.FirstOrDefault(x => x.UserId == user.Id && (x.AccountId == account.Id || x.ParentAccountId == account.Id));
            var identityRole = await _roleManager.FindByIdAsync(userAccount.RoleId);
            return identityRole;
        }

        public IdentityResult RemoveUserFromAccount(IdentityUserExtended user, Account account)
        {
            try
            {
                var userAccounts = _context.UserAccounts;
                var userAccount = userAccounts.FirstOrDefault(x => x.UserId == user.Id && x.ParentAccountId == account.Id);
                userAccounts.Remove(userAccount);
                _context.SaveChanges();
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = ex.Message });
            }
        }

        public async Task<IdentityResult> TryAddUserToAccount(string userId, string role, string accountName, string entryAccountName = null, bool isAppStoreAccount = false)
        {
            try
            {
                var account = _accountsManager.TryAddAccount(accountName, isAppStoreAccount);
                var userAccounts = _context.UserAccounts;
                var identityRole = await _roleManager.FindByNameAsync(role);
                var userAccount = new UserAccount
                {
                    Id = Guid.NewGuid().ToString(),
                    AccountId = account.Id,
                    ParentAccountId = account.Id,
                    UserId = userId,
                    RoleId = identityRole.Id,
                    IsOwner = account.IsAppStoreAccount
                };

                if (!string.IsNullOrEmpty(entryAccountName))
                {
                    var entryAccount = _accountsManager.TryAddAccount(entryAccountName);
                    userAccount.ParentAccountId = entryAccount.Id;
                    userAccount.IsOwner = entryAccount.IsAppStoreAccount;
                }

                if (userAccounts.ToList().Any(x => x.IsAssigned(userAccount)))
                {
                    return IdentityResult.Failed(new IdentityError { Description = "Warning! The user is already assigned to this account" });
                }

                userAccounts.Add(userAccount);
                _context.SaveChanges();
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = ex.Message });
            }
        }
    }
}

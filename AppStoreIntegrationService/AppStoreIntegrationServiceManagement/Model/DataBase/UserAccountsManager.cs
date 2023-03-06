using AppStoreIntegrationServiceManagement.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;

namespace AppStoreIntegrationServiceManagement.Model.DataBase
{
    public class UserAccountsManager
    {
        private readonly AppStoreIntegrationServiceContext _context;
        private readonly AccountsManager _accountsManager;
        private readonly UserManager<IdentityUserExtended> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserAccountsManager
        (
            AppStoreIntegrationServiceContext context,
            AccountsManager accountsManager,
            UserManager<IdentityUserExtended> userManager,
            RoleManager<IdentityRole> roleManager
        )
        {
            _context = context;
            _accountsManager = accountsManager;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IdentityResult> TryAddUserToAccount(IdentityUserExtended user, string roleId, string accountName = null)
        {
            try
            {
                await AddUserToAccount(user, user.UserName, roleId);

                if (string.IsNullOrEmpty(accountName))
                {
                    return IdentityResult.Success;
                }

                await AddUserToAccount(user, accountName, roleId);
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = ex.Message });
            }
        }

        public IEnumerable<Account> GetUserAccounts(IdentityUserExtended user)
        {
            var userAccounts = _context.UserAccounts.ToList();

            foreach (var userAccount in userAccounts.Where(x => x.UserId == user.Id))
            {
                yield return _accountsManager.GetAccountById(userAccount.AccountId);
            }
        }

        public IdentityResult RemoveUserFromAccounts(IdentityUserExtended user)
        {
            try
            {
                var userAccounts = _context.UserAccounts;
                var accounts = _accountsManager.GetAllAccounts();

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

        public bool HasFullAccess(IdentityUserExtended user)
        {
            var userAccounts = _context.UserAccounts.ToList();
            var account = _accountsManager.GetAccountById(user.SelectedAccountId);
            var userAccount = userAccounts.FirstOrDefault(x => x.UserId == user.Id && x.AccountId == account.Id);
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
            var account = _accountsManager.GetAccountById(owner.SelectedAccountId);
            var userAccounts = _context.UserAccounts.ToList();
            return userAccounts.Any(x => x.AccountId == account.Id && x.UserId == member.Id);
        }

        public bool BelongsTo(IdentityUserExtended user, string accountId)
        {
            var userAccounts = _context.UserAccounts.ToList();
            return userAccounts.Any(x => x.AccountId == accountId && x.UserId == user.Id);
        }

        public bool IsInRole(IdentityUserExtended user, string roleId)
        {
            var userAccounts = _context.UserAccounts.ToList();
            return userAccounts.Any(x => x.AccountId == user.SelectedAccountId && x.UserId == user.Id && x.RoleId == roleId);
        }

        public async Task<IdentityRole> GetUserRoleForAccount(IdentityUserExtended user, Account account)
        {
            var userAccounts = _context.UserAccounts.ToList();
            var userAccount = userAccounts.FirstOrDefault(x => x.UserId == user.Id && x.AccountId == account.Id);
            var identityRole = await _roleManager.FindByIdAsync(userAccount.RoleId);
            return identityRole;
        }

        public IdentityResult RemoveUserFromAccount(IdentityUserExtended user, Account account)
        {
            try
            {
                var userAccounts = _context.UserAccounts;
                var userAccount = userAccounts.FirstOrDefault(x => x.UserId == user.Id && x.AccountId == account.Id);
                userAccounts.Remove(userAccount);
                _context.SaveChanges();
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = ex.Message });
            }
        }

        private async Task AddUserToAccount(IdentityUserExtended user, string accountName, string roleId)
        {
            var account = _accountsManager.TryAddAccount(accountName);
            var userAccounts = _context.UserAccounts;

            if (userAccounts.ToList().Any(x => x.AccountId == account.Id && x.UserId == user.Id))
            {
                return;
            }

            userAccounts.Add(new UserAccount
            {
                Id = Guid.NewGuid().ToString(),
                AccountId = account.Id,
                UserId = user.Id,
                RoleId = roleId,
                IsOwner = !userAccounts.Any(x => x.AccountId == account.Id)
            });

            _context.SaveChanges();
        }
    }
}

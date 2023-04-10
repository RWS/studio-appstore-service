using AppStoreIntegrationServiceCore.DataBase.Models;
using AppStoreIntegrationServiceManagement.DataBase.Models;
using Microsoft.AspNetCore.Identity;

namespace AppStoreIntegrationServiceManagement.DataBase.Interface
{
    public interface IUserAccountsManager
    {
        IEnumerable<Account> GetUserAccounts(UserProfile user);
        bool CanBeRemoved(UserProfile user);
        Task<IdentityResult> RemoveUserFromAllAccounts(UserProfile user);
        bool BelongsTo(UserProfile member, Account account);
        IEnumerable<UserProfile> GetUsersFromAccount(Account account);
        UserRole GetUserRoleForAccount(UserProfile user, Account account);
        Task<IdentityResult> RemoveUserFromAccount(UserProfile user, Account account);
        Task<IdentityResult> TryAddUserToAccount(UserAccount userAccount);
        Account GetUserUnsyncedAccount(UserProfile user);
    }
}

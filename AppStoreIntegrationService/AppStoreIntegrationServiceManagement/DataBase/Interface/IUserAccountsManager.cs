using AppStoreIntegrationServiceCore.DataBase.Models;
using AppStoreIntegrationServiceManagement.DataBase.Models;
using Microsoft.AspNetCore.Identity;

namespace AppStoreIntegrationServiceManagement.DataBase.Interface
{
    public interface IUserAccountsManager
    {
        Task<IdentityResult> ChangeUserRoleForAccount(UserProfile userProfile, Account account, string role);
        Task<IdentityResult> RemoveUserFromAccount(UserProfile user, Account account);
        bool CanBeRemovedFromAccount(UserProfile user, Account account);
        UserRole GetUserRoleForAccount(UserProfile user, Account account);
        Task<IdentityResult> TryAddUserToAccount(UserAccount userAccount);
        Task<IdentityResult> RemoveUserFromAllAccounts(UserProfile user);
        IEnumerable<UserProfile> GetUsersFromAccount(Account account);
        IEnumerable<Account> GetUserAccounts(UserProfile user);
        bool BelongsTo(UserProfile member, Account account = null);
        Account GetUserUnsyncedAccount(UserProfile user);
    }
}

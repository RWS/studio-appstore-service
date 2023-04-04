using AppStoreIntegrationServiceCore.DataBase.Models;
using Microsoft.AspNetCore.Identity;

namespace AppStoreIntegrationServiceCore.DataBase.Interface
{
    public interface IUserAccountsManager
    {
        IEnumerable<Account> GetUserAccounts(UserProfile user);
        bool CanBeRemoved(UserProfile user);
        IdentityResult RemoveUserAccounts(UserProfile user);
        bool BelongsTo(UserProfile member, Account account);
        UserRole GetUserRoleForAccount(UserProfile user, Account account);
        IdentityResult RemoveUserFromAccount(UserProfile user, Account account);
        IdentityResult TryAddUserToAccount(UserAccount userAccount);
    }
}

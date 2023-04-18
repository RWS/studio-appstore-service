using AppStoreIntegrationServiceCore.DataBase.Models;
using Microsoft.AspNetCore.Identity;

namespace AppStoreIntegrationServiceManagement.DataBase.Interface
{
    public interface IAccountsManager
    {
        Task<Account> TryAddAccount(Account account);
        Account GetAccountById(string id);
        Account GetAccountByName(string name);
        Task<IdentityResult> RemoveAccountById(string id);
        Task<Account> TryUpdateAccountName(Account account, string name);
    }
}
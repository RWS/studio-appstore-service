using AppStoreIntegrationServiceCore.DataBase.Models;

namespace AppStoreIntegrationServiceManagement.DataBase.Interface
{
    public interface IAccountsManager
    {
        Task<Account> TryAddAccount(Account account);
        Account GetAccountById(string id);
        Account GetAccountByName(string name);
        Task<Account> TryUpdateAccountName(Account account, string name);
    }
}

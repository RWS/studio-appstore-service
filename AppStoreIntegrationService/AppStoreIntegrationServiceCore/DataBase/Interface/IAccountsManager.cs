using AppStoreIntegrationServiceCore.DataBase.Models;

namespace AppStoreIntegrationServiceCore.DataBase.Interface
{
    public interface IAccountsManager
    {
        Account TryAddAccount(Account account);
        Account GetAccountById(string id);
        Account GetAccountByName(string name);
        void UpdateAccountName(Account account, string name);
    }
}

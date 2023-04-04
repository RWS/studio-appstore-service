using AppStoreIntegrationServiceCore.DataBase.Models;

namespace AppStoreIntegrationServiceCore.DataBase.Interface
{
    public interface IAccountsManager
    {
        Account TryAddAccount(Account account);
        Account GetAccountById(string id);
    }
}

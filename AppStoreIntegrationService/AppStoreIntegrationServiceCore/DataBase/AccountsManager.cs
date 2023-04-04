using AppStoreIntegrationServiceCore.DataBase.Interface;
using AppStoreIntegrationServiceCore.DataBase.Models;

namespace AppStoreIntegrationServiceCore.DataBase
{
    public class AccountsManager : IAccountsManager
    {
        private readonly IServiceContextFactory _serviceContext;

        public AccountsManager(IServiceContextFactory serviceContext)
        {
            _serviceContext = serviceContext;
        }

        public Account TryAddAccount(Account account)
        {
            if (account == null)
            {
                return null;
            }

            using var context = _serviceContext.CreateContext();
            var existingAccount = context.Accounts.FirstOrDefault(x => x.Name == account.Name);

            if (existingAccount != null)
            {
                return existingAccount;
            }

            context.Accounts.Add(account);
            context.SaveChanges();
            return account;
        }

        public Account GetAccountById(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            using var context = _serviceContext.CreateContext();
            return context.Accounts.ToList().FirstOrDefault(x => x.Id == id);
        }
    }
}

using AppStoreIntegrationServiceCore.DataBase.Interface;
using AppStoreIntegrationServiceCore.DataBase.Models;
using AppStoreIntegrationServiceManagement.DataBase.Interface;
using Microsoft.AspNetCore.Identity;

namespace AppStoreIntegrationServiceManagement.DataBase
{
    public class AccountsManager : Manager, IAccountsManager
    {
        private readonly IServiceContextFactory _serviceContext;

        public AccountsManager(IServiceContextFactory serviceContext)
        {
            _serviceContext = serviceContext;
        }

        public async Task<Account> TryAddAccount(Account account)
        {
            if (ExistNullParams(out _, account?.Id))
            {
                return null;
            }

            using (var context = _serviceContext.CreateContext())
            {
                var existingAccount = context.Accounts.FirstOrDefault(x => x.Name == account.Name || x.Id == account.Id);

                if (existingAccount != null)
                {
                    return existingAccount;
                }

                context.Accounts.Add(account);
                await context.SaveChangesAsync();
                return account;
            }
        }

        public async Task<Account> TryUpdateAccountName(Account account, string name)
        {
            if (ExistNullParams(out _, name))
            {
                return account;
            }

            try
            {
                using (var context = _serviceContext.CreateContext())
                {
                    var oldAccount = context.Accounts.FirstOrDefault(x => x.Id == account.Id);
                    oldAccount.Name = name;
                    await context.SaveChangesAsync();
                    return oldAccount;
                }
            }
            catch (Exception)
            {
                return account;
            }
        }

        public Account GetAccountById(string id)
        {
            using (var context = _serviceContext.CreateContext())
            {
                return context.Accounts.ToList().FirstOrDefault(x => x.Id == id);
            }
        }

        public Account GetAccountByName(string name)
        {
            if (ExistNullParams(out _, name))
            {
                return null;
            }

            using (var context = _serviceContext.CreateContext())
            {
                var accounts = context.Accounts.ToList();
                var account = accounts.FirstOrDefault(x => x.Name == name);
                return account;
            }
        }

        public async Task<IdentityResult> RemoveAccountById(string id)
        {
            if (ExistNullParams(out var result, id))
            {
                return result;
            }

            try
            {
                using (var context = _serviceContext.CreateContext())
                {
                    var accounts = context.Accounts;
                    var account = accounts.FirstOrDefault(x => x.Id == id);
                    accounts.Remove(account);
                    await context.SaveChangesAsync();
                }

                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = ex.Message });
            }
        }
    }
}
using AppStoreIntegrationServiceManagement.Areas.Identity.Data;

namespace AppStoreIntegrationServiceManagement.Model.DataBase
{
    public class AccountsManager
    {
        private readonly AppStoreIntegrationServiceContext _context;

        public AccountsManager(AppStoreIntegrationServiceContext context)
        {
            _context = context;
        }

        public Account TryAddAccount(string accountName)
        {
            var accounts = _context.Accounts;
            var account = accounts.ToList().FirstOrDefault(x => x.AccountName == accountName);

            if (account != null)
            {
                return account;
            }

            account = new Account { Id = Guid.NewGuid().ToString(), AccountName = accountName };
            accounts.Add(account);
            _context.SaveChanges();
            return account;
        }

        public void UpdateAccount(Account account)
        {
            var accounts = _context.Accounts;
            var oldAccount = accounts.FirstOrDefault(x => x.Id == account.Id);
            oldAccount.AccountName = account.AccountName;
           _context.SaveChanges();
        }

        public Account GetAccountById(string id)
        {
            return _context.Accounts.ToList().FirstOrDefault(x => x.Id == id);
        }

        public IEnumerable<Account> GetAllAccounts()
        {
            return _context.Accounts.ToList();
        }

        public void RemoveAccountById(string id)
        {
            var accounts = _context.Accounts;
            accounts.Remove(accounts.FirstOrDefault(x => x.Id == id));
        }
    }
}

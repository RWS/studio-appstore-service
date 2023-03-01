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

        public Account GetAccountById(string id)
        {
            return _context.Accounts.ToList().FirstOrDefault(x => x.Id == id);
        }

        public IEnumerable<Account> GetAllAccounts()
        {
            return _context.Accounts.ToList();
        }

        public void RemoveRange(IEnumerable<Account> toRemove)
        {
            var accounts = _context.Accounts;
            accounts.RemoveRange(accounts.Where(x => toRemove.Any(y => y.Id == x.Id)));
            _context.SaveChanges();
        }
    }
}

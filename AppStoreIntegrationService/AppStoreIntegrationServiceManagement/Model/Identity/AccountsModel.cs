using AppStoreIntegrationServiceCore.DataBase.Models;

namespace AppStoreIntegrationServiceManagement.Model.Identity
{
    public class AccountsModel
    {
        public IEnumerable<Account> Accounts { get; set; }
        public string ReturnUrl { get; set; }
        public string SelectedAccountId { get; set; }
        public bool RememberMyChoice { get; set; }
    }
}

using Microsoft.AspNetCore.Mvc;

namespace AppStoreIntegrationServiceManagement.Filters
{
    public class AccountSelectedAttribute : TypeFilterAttribute
    {
        public AccountSelectedAttribute() : base(typeof(AccountSelectedFilter)) { }
    }
}

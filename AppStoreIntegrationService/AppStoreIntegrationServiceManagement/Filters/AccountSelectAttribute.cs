using Microsoft.AspNetCore.Mvc;

namespace AppStoreIntegrationServiceManagement.Filters
{
    public class AccountSelectAttribute : TypeFilterAttribute
    {
        public AccountSelectAttribute() : base(typeof(AccountSelectFilter)) { }
    }
}

using Microsoft.AspNetCore.Mvc;

namespace AppStoreIntegrationServiceManagement.Filters
{
    public class OwnerAttribute : TypeFilterAttribute
    {
        public OwnerAttribute() : base(typeof(OwnerFilter)) { }
    }
}

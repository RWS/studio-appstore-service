using Microsoft.AspNetCore.Mvc;

namespace AppStoreIntegrationServiceManagement.Filters
{
    public class SyncDBAttribute : TypeFilterAttribute
    {
        public SyncDBAttribute() : base(typeof(SyncDBFilter)) { }
    }
}

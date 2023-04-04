using Microsoft.AspNetCore.Mvc;

namespace AppStoreIntegrationServiceManagement.Filters
{
    public class DBSynchedAttribute : TypeFilterAttribute
    {
        public DBSynchedAttribute() : base(typeof(DBSynchedFilter)) { }
    }
}

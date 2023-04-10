using AppStoreIntegrationServiceCore.DataBase.Models;

namespace AppStoreIntegrationServiceManagement.DataBase.Interface
{
    public interface IAccountEntitlementsManager
    {
        Task Add(AccountEntitlement entitlement);
    }
}

using AppStoreIntegrationServiceCore.DataBase.Models;
using Microsoft.AspNetCore.Identity;

namespace AppStoreIntegrationServiceManagement.DataBase.Interface
{
    public interface IAccountEntitlementsManager
    {
        Task<IdentityResult> TryAddEntitlement(AccountEntitlement entitlement);
        Task<IdentityResult> RemoveByAccountId(string accountId);
    }
}

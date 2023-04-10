using AppStoreIntegrationServiceCore.DataBase.Interface;
using AppStoreIntegrationServiceCore.DataBase.Models;
using AppStoreIntegrationServiceManagement.DataBase.Interface;

namespace AppStoreIntegrationServiceManagement.DataBase
{
    public class AccountEntitlementsManager : IAccountEntitlementsManager
    {
        private readonly IServiceContextFactory _serviceContext;

        public AccountEntitlementsManager(IServiceContextFactory serviceContext)
        {
            _serviceContext = serviceContext;
        }

        public async Task Add(AccountEntitlement entitlement)
        {
            if (string.IsNullOrEmpty(entitlement?.Id) || string.IsNullOrEmpty(entitlement?.AccountId))
            {
                return;
            }

            using (var context = _serviceContext.CreateContext())
            {
                if (context.AccountEntitlements.ToList().Any(x => x.AccountId == entitlement.AccountId || x.Id == entitlement.Id))
                {
                    return;
                }

                context.AccountEntitlements.Add(entitlement);
                await context.SaveChangesAsync();
            }
        }
    }
}

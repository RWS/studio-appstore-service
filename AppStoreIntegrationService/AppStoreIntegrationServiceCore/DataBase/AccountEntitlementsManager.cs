using AppStoreIntegrationServiceCore.DataBase.Interface;
using AppStoreIntegrationServiceCore.DataBase.Models;

namespace AppStoreIntegrationServiceCore.DataBase
{
    public class AccountEntitlementsManager : IAccountEntitlementsManager
    {
        private readonly IServiceContextFactory _serviceContext;

        public AccountEntitlementsManager(IServiceContextFactory serviceContext)
        {
            _serviceContext = serviceContext;
        }

        public IEnumerable<AccountEntitlement> Entitlements 
        {
            get
            {
                using var context = _serviceContext.CreateContext();
                return context.AccountEntitlements.ToList();
            }
        }

        public void Add(AccountEntitlement entitlement)
        {
            using var context = _serviceContext.CreateContext();
            if (context.AccountEntitlements.ToList().Any(x => x.AccountId == entitlement.AccountId))
            {
                return;
            }

            context.AccountEntitlements.Add(entitlement);
            context.SaveChanges();
        }
    }
}

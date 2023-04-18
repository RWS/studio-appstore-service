using AppStoreIntegrationServiceCore.DataBase.Interface;
using AppStoreIntegrationServiceCore.DataBase.Models;
using AppStoreIntegrationServiceManagement.DataBase.Interface;
using Microsoft.AspNetCore.Identity;

namespace AppStoreIntegrationServiceManagement.DataBase
{
    public class AccountEntitlementsManager : Manager, IAccountEntitlementsManager
    {
        private readonly IServiceContextFactory _serviceContext;

        public AccountEntitlementsManager(IServiceContextFactory serviceContext)
        {
            _serviceContext = serviceContext;
        }

        public async Task<IdentityResult> TryAddEntitlement(AccountEntitlement entitlement)
        {
            if (ExistNullParams(out var result, entitlement?.Id, entitlement?.AccountId))
            {
                return result;
            }

            try
            {
                using (var context = _serviceContext.CreateContext())
                {
                    if (context.AccountEntitlements.ToList().Any(x => x.AccountId == entitlement.AccountId || x.Id == entitlement.Id))
                    {
                        return IdentityResult.Success;
                    }

                    context.AccountEntitlements.Add(entitlement);
                    await context.SaveChangesAsync();
                }

                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = ex.Message });
            }
        }

        public async Task<IdentityResult> RemoveByAccountId(string accountId)
        {
            if (ExistNullParams(out var result, accountId))
            {
                return result;
            }

            try
            {
                using (var context = _serviceContext.CreateContext())
                {
                    var entitlement = context.AccountEntitlements.FirstOrDefault(x => x.AccountId == accountId);
                    context.AccountEntitlements.Remove(entitlement);
                    await context.SaveChangesAsync();
                }

                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = ex.Message });
            }

        }
    }
}
using AppStoreIntegrationServiceCore.DataBase.Interface;
using AppStoreIntegrationServiceCore.DataBase.Models;
using AppStoreIntegrationServiceManagement.DataBase.Interface;
using Microsoft.AspNetCore.Identity;

namespace AppStoreIntegrationServiceManagement.DataBase
{
    public class AccountAgreementsManager : Manager, IAccountAgreementsManager
    {
        private readonly IServiceContextFactory _serviceContext;

        public AccountAgreementsManager(IServiceContextFactory serviceContext)
        {
            _serviceContext = serviceContext;
        }

        public async Task<IdentityResult> TryAddAgreement(AccountAgreement agreement)
        {
            if (ExistNullParams(out var result, agreement?.AccountId, agreement?.UserProfileId))
            {
                return result;
            }

            try
            {
                using (var context = _serviceContext.CreateContext())
                {
                    var agreements = context.AccountAgreements.ToList();
                    if (agreements.Any(x => x.AccountId == agreement.AccountId && x.UserProfileId == agreement.UserProfileId))
                    {
                        return IdentityResult.Success;
                    }

                    context.AccountAgreements.Add(agreement);
                    await context.SaveChangesAsync();
                }

                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = ex.Message });
            }
        }

        public async Task<IdentityResult> Remove(UserProfile user, Account account)
        {
            if (ExistNullParams(out var result, user?.Id, account?.Id))
            {
                return result;
            }

            try
            {
                using (var context = _serviceContext.CreateContext())
                {
                    var agreement = context.AccountAgreements.FirstOrDefault(x => x.UserProfileId == user.Id && x.AccountId == account.Id);
                    
                    if (agreement == null)
                    {
                        return IdentityResult.Success;
                    }

                    context.AccountAgreements.Remove(agreement);
                    await context.SaveChangesAsync();
                }

                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = ex.Message });
            }
        }

        public bool HasAggreement(UserProfile user, Account account)
        {
            if (ExistNullParams(out _, user?.Id, account?.Id))
            {
                return false;
            }

            using (var context = _serviceContext.CreateContext())
            {
                var agreements = context.AccountAgreements.ToList();
                return agreements.Any(x => x.UserProfileId == user.Id && x.AccountId == account.Id);
            }
        }
    }
}
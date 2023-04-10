using AppStoreIntegrationServiceCore.DataBase.Interface;
using AppStoreIntegrationServiceCore.DataBase.Models;
using AppStoreIntegrationServiceManagement.DataBase.Interface;

namespace AppStoreIntegrationServiceManagement.DataBase
{
    public class AccountAgreementsManager : IAccountAgreementsManager
    {
        private readonly IServiceContextFactory _serviceContext;

        public AccountAgreementsManager(IServiceContextFactory serviceContext)
        {
            _serviceContext = serviceContext;
        }

        public async Task Add(AccountAgreement agreement)
        {
            if (agreement?.AnyNull() ?? true)
            {
                return;
            }

            using (var context = _serviceContext.CreateContext())
            {
                var agreements = context.AccountAgreements.ToList();
                if (agreements.Any(x => x.AccountId == agreement.AccountId && x.UserProfileId == agreement.UserProfileId))
                {
                    return;
                }

                context.AccountAgreements.Add(agreement);
                await context.SaveChangesAsync();
            }
        }

        public async Task Remove(UserProfile user)
        {
            if (string.IsNullOrEmpty(user?.Id))
            {
                return;
            }

            using (var context = _serviceContext.CreateContext())
            {
                var agreements = context.AccountAgreements.Where(x => x.UserProfileId == user.Id);
                context.AccountAgreements.RemoveRange(agreements);
                await context.SaveChangesAsync();
            }
        }

        public async Task Remove(UserProfile user, Account account)
        {
            if (string.IsNullOrEmpty(user?.Id) || string.IsNullOrEmpty(account?.Id))
            {
                return;
            }

            using (var context = _serviceContext.CreateContext())
            {
                var agreement = context.AccountAgreements.FirstOrDefault(x => x.UserProfileId == user.Id && x.AccountId == account.Id);
                context.AccountAgreements.Remove(agreement);
                await context.SaveChangesAsync();
            }
        }

        public bool HasAggreement(UserProfile user, Account account)
        {
            using (var context = _serviceContext.CreateContext())
            {
                var agreements = context.AccountAgreements.ToList();
                return agreements.Any(x => x.UserProfileId == user.Id && x.AccountId == account.Id);
            }
        }
    }
}

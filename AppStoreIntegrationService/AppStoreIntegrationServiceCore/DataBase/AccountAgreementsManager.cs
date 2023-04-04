using AppStoreIntegrationServiceCore.DataBase.Interface;
using AppStoreIntegrationServiceCore.DataBase.Models;

namespace AppStoreIntegrationServiceCore.DataBase
{
    public class AccountAgreementsManager : IAccountAgreementsManager
    {
        private readonly IServiceContextFactory _serviceContext;

        public AccountAgreementsManager(IServiceContextFactory serviceContext)
        {
            _serviceContext = serviceContext;
        }

        public void Add(AccountAgreement agreement)
        {
            using var context = _serviceContext.CreateContext();
            var agreements = context.AccountAgreements.ToList();
            if (agreements.Any(x => x.AccountId == agreement.AccountId && x.UserProfileId == agreement.UserProfileId))
            {
                return;
            }

            context.AccountAgreements.Add(agreement);
            context.SaveChanges();
        }

        public void Remove(UserProfile user)
        {
            using var context = _serviceContext.CreateContext();
            var agreement = context.AccountAgreements.FirstOrDefault(x => x.UserProfileId == user.Id);
            context.AccountAgreements.Remove(agreement);
            context.SaveChanges();
        }

        public void Remove(UserProfile user, Account account)
        {
            using var context = _serviceContext.CreateContext();
            var agreement = context.AccountAgreements.FirstOrDefault(x => x.UserProfileId == user.Id && x.AccountId == account.Id);
            context.AccountAgreements.Remove(agreement);
            context.SaveChanges();
        }

        public bool HasAggreement(UserProfile user, Account account)
        {
            using var context = _serviceContext.CreateContext();
            var agreements = context.AccountAgreements.ToList();
            return agreements.Any(x => x.UserProfileId == user.Id && x.AccountId == account.Id);
        }
    }
}

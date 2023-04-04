using AppStoreIntegrationServiceCore.DataBase.Models;

namespace AppStoreIntegrationServiceCore.DataBase.Interface
{
    public interface IAccountAgreementsManager
    {
        void Add(AccountAgreement agreement);
        void Remove(UserProfile user);
        void Remove(UserProfile user, Account account);
        bool HasAggreement(UserProfile user, Account account);
    }
}

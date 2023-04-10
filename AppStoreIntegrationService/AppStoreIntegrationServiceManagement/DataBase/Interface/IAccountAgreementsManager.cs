using AppStoreIntegrationServiceCore.DataBase.Models;

namespace AppStoreIntegrationServiceManagement.DataBase.Interface
{
    public interface IAccountAgreementsManager
    {
        Task Add(AccountAgreement agreement);
        Task Remove(UserProfile user);
        Task Remove(UserProfile user, Account account);
        bool HasAggreement(UserProfile user, Account account);
    }
}

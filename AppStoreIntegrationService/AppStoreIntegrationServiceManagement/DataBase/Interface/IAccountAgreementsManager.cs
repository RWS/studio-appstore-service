using AppStoreIntegrationServiceCore.DataBase.Models;
using Microsoft.AspNetCore.Identity;

namespace AppStoreIntegrationServiceManagement.DataBase.Interface
{
    public interface IAccountAgreementsManager
    {
        Task<IdentityResult> TryAddAgreement(AccountAgreement agreement);
        Task<IdentityResult> Remove(UserProfile user, Account account);
        bool HasAggreement(UserProfile user, Account account);
    }
}
using AppStoreIntegrationServiceManagement.Model.DataBase;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AppStoreIntegrationServiceManagement.Model
{
    public class CustomController : Controller
    {
        private CustomClaimsPrincipal _extendedUser;

        private IServiceProvider ServiceProvider
        {
            get => HttpContext.RequestServices;
        }

        private UserManager<IdentityUserExtended> UserManager
        {
            get => ServiceProvider.GetRequiredService<UserManager<IdentityUserExtended>>();
        }

        private UserAccountsManager UserAccountsManager
        {
            get => ServiceProvider.GetRequiredService<UserAccountsManager>();
        }

        private RoleManager<IdentityRole> RoleManager
        {
            get => ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        }

        private AccountsManager AccountsManager
        {
            get => ServiceProvider.GetRequiredService<AccountsManager>();
        }

        public CustomClaimsPrincipal ExtendedUser
        {
            get
            {
                return _extendedUser ?? new CustomClaimsPrincipal
                (
                    User,
                    UserManager,
                    RoleManager,
                    UserAccountsManager,
                    AccountsManager
                );
            }
            protected set
            {
                _extendedUser = value;
            }
        }
    }
}

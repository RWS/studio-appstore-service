using AppStoreIntegrationServiceCore.DataBase;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;

namespace AppStoreIntegrationServiceManagement.Model
{
    public abstract class CustomRazorPage<TModel> : RazorPage<TModel>
    {
        private CustomClaimsPrincipal _extendedUser;

        private IServiceProvider ServiceProvider
        {
            get => Context.RequestServices;
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

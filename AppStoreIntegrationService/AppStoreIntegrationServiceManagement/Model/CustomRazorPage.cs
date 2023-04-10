using AppStoreIntegrationServiceCore.DataBase.Interface;
using AppStoreIntegrationServiceManagement.DataBase;
using AppStoreIntegrationServiceManagement.DataBase.Interface;
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

        private IUserProfilesManager UserManager 
        {
            get => ServiceProvider.GetRequiredService<IUserProfilesManager>();
        }

        private IUserAccountsManager UserAccountsManager
        {
            get => ServiceProvider.GetRequiredService<IUserAccountsManager>();
        }

        private IAccountsManager AccountsManager
        {
            get => ServiceProvider.GetRequiredService<IAccountsManager>();
        }

        public CustomClaimsPrincipal ExtendedUser
        {
            get
            {
                return _extendedUser ?? new CustomClaimsPrincipal
                (
                    User,
                    UserManager,
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

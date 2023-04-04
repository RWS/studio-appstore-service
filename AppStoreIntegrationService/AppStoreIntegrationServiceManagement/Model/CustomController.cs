using AppStoreIntegrationServiceCore.DataBase;
using AppStoreIntegrationServiceCore.DataBase.Interface;
using AppStoreIntegrationServiceManagement.Model.Identity.Interface;
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

        protected IUserProfilesManager UserManager
        {
            get => ServiceProvider.GetRequiredService<IUserProfilesManager>();
        }

        protected IAccountAgreementsManager AccountAgreementsManager
        {
            get => ServiceProvider.GetRequiredService<IAccountAgreementsManager>();
        }

        protected IUserAccountsManager UserAccountsManager
        {
            get => ServiceProvider.GetRequiredService<IUserAccountsManager>();
        }

        protected IUserRolesManager RoleManager
        {
            get => ServiceProvider.GetRequiredService<IUserRolesManager>();
        }

        protected IAuth0UserManager Auth0UserManager
        {
            get => ServiceProvider.GetRequiredService<IAuth0UserManager>();
        }

        protected IAccountsManager AccountsManager
        {
            get => ServiceProvider.GetRequiredService<IAccountsManager>();
        }

        protected IHttpContextAccessor ContextAccessor
        {
            get => ServiceProvider.GetRequiredService<IHttpContextAccessor>();
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

        protected string GetUrlBase()
        {
            var request = ContextAccessor.HttpContext.Request;
            return $"{request.Scheme}://{request.Host.Value}";
        }
    }
}

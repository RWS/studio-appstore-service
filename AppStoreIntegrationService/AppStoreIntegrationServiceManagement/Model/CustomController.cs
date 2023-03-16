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

        protected UserManager<IdentityUserExtended> UserManager
        {
            get => ServiceProvider.GetRequiredService<UserManager<IdentityUserExtended>>();
        }

        protected UserAccountsManager UserAccountsManager
        {
            get => ServiceProvider.GetRequiredService<UserAccountsManager>();
        }

        protected RoleManager<IdentityRole> RoleManager
        {
            get => ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        }

        protected AccountsManager AccountsManager
        {
            get => ServiceProvider.GetRequiredService<AccountsManager>();
        }

        protected SignInManager<IdentityUserExtended> SignInManager
        {
            get => ServiceProvider.GetRequiredService<SignInManager<IdentityUserExtended>>();
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

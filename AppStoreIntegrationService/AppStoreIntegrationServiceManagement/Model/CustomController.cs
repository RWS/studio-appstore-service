using AppStoreIntegrationServiceCore.DataBase.Interface;
using AppStoreIntegrationServiceManagement.DataBase;
using AppStoreIntegrationServiceManagement.DataBase.Interface;
using Microsoft.AspNetCore.Mvc;

namespace AppStoreIntegrationServiceManagement.Model
{
    public class CustomController : Controller
    {
        private readonly IUserProfilesManager _userProfilesManager;
        private readonly IUserAccountsManager _userAccountsManager;
        private readonly IAccountsManager _accountsManager;
        private CustomClaimsPrincipal _extendedUser;

        public CustomController
        (
            IUserProfilesManager userProfilesManager,
            IUserAccountsManager userAccountsManager,
            IAccountsManager accountsManager
        ) : base()
        {
            _userProfilesManager = userProfilesManager;
            _userAccountsManager = userAccountsManager;
            _accountsManager = accountsManager;
        }

        public CustomClaimsPrincipal ExtendedUser 
        { 
            get
            {
                _extendedUser ??= new CustomClaimsPrincipal
                (
                    User,
                    _userProfilesManager,
                    _userAccountsManager,
                    _accountsManager
                );

                return _extendedUser;
            }
        }

        protected string GetUrlBase()
        {
            var request = HttpContext.Request;
            return $"{request.Scheme}://{request.Host.Value}";
        }
    }
}

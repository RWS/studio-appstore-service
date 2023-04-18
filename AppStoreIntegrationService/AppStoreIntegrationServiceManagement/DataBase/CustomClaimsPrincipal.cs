using AppStoreIntegrationServiceCore.DataBase.Interface;
using AppStoreIntegrationServiceManagement.DataBase.Interface;
using System.Security.Claims;
using System.Security.Principal;

namespace AppStoreIntegrationServiceManagement.DataBase
{
    public class CustomClaimsPrincipal : ClaimsPrincipal
    {
        private readonly IUserProfilesManager _userProfilesManager;
        private readonly IUserAccountsManager _userAccountsManager;
        private readonly IAccountsManager _accountsManager;

        public CustomClaimsPrincipal
        (
            IPrincipal principal,
            IUserProfilesManager userProfilesManager,
            IUserAccountsManager userAccountsManager,
            IAccountsManager accountsManager
        ) : base(principal)
        {
            _userProfilesManager = userProfilesManager;
            _userAccountsManager = userAccountsManager;
            _accountsManager = accountsManager;
        }

        public string AccountName { get => GetAccountName(); }

        public string Role { get => GetRole(); }

        public bool PushNotificationsEnabled { get => HasPushNotificationsEnabled(); }

        public override bool IsInRole(string roleName)
        {
            var user = _userProfilesManager?.GetUser(this);

            if (user == null)
            {
                return false;
            }

            var account = _accountsManager.GetAccountById(user.SelectedAccountId);

            if (account == null)
            {
                return false;
            }

            var userRole = _userAccountsManager.GetUserRoleForAccount(user, account);
            return userRole?.Name == roleName;
        }

        public bool IsInRoles(params string[] roles)
        {
            var user = _userProfilesManager?.GetUser(this);

            if (user == null)
            {
                return false;
            }

            var account = _accountsManager.GetAccountById(user.SelectedAccountId);

            if (account == null)
            {
                return false;
            }

            var userRole = _userAccountsManager.GetUserRoleForAccount(user, account);
            return roles.Any(x => x == userRole?.Name);
        }

        public bool HasSelectedAccount()
        {
            var user = _userProfilesManager?.GetUser(this);
            return !string.IsNullOrEmpty(user?.SelectedAccountId);
        }

        private string GetAccountName()
        {
            var user = _userProfilesManager?.GetUser(this);
            var account = _accountsManager?.GetAccountById(user?.SelectedAccountId);
            return account?.Name;
        }

        private string GetRole()
        {
            var user = _userProfilesManager?.GetUser(this);
            var account = _accountsManager?.GetAccountById(user?.SelectedAccountId);
            var role = _userAccountsManager?.GetUserRoleForAccount(user, account);
            return role?.Name;
        }

        private bool HasPushNotificationsEnabled()
        {
            var user = _userProfilesManager?.GetUser(this);
            return user.PushNotificationsEnabled;
        }
    }
}

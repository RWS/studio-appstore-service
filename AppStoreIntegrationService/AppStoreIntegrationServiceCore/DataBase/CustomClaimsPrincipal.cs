using AppStoreIntegrationServiceCore.DataBase.Interface;
using System.Security.Claims;
using System.Security.Principal;

namespace AppStoreIntegrationServiceCore.DataBase
{
    public class CustomClaimsPrincipal : ClaimsPrincipal
    {
        private readonly IUserProfilesManager _userManager;
        private readonly IUserAccountsManager _userAccountsManager;
        private readonly IAccountsManager _accountsManager;

        public CustomClaimsPrincipal
        (
            IPrincipal principal,
            IUserProfilesManager userManager,
            IUserAccountsManager userAccountsManager,
            IAccountsManager accountsManager
        ) : base(principal)
        {
            _userManager = userManager;
            _userAccountsManager = userAccountsManager;
            _accountsManager = accountsManager;
        }

        public string AccountName { get => GetAccountName(); }

        public string Role { get => GetRole(); }

        public bool PushNotificationsEnabled { get => HasPushNotificationsEnabled(); }

        public override bool IsInRole(string roleName)
        {
            var user = _userManager?.GetUser(this);

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
            var user = _userManager?.GetUser(this);

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
            var user = _userManager?.GetUser(this);
            return !string.IsNullOrEmpty(user?.SelectedAccountId);
        }

        private string GetAccountName()
        {
            var user = _userManager?.GetUser(this);
            var account = _accountsManager?.GetAccountById(user?.SelectedAccountId);
            return account?.Name;
        }

        private string GetRole()
        {
            var user = _userManager?.GetUser(this);
            var account = _accountsManager?.GetAccountById(user?.SelectedAccountId);
            var role = _userAccountsManager?.GetUserRoleForAccount(user, account);
            return role?.Name;
        }

        private bool HasPushNotificationsEnabled()
        {
            var user = _userManager?.GetUser(this);
            return user.PushNotificationsEnabled;
        }
    }
}

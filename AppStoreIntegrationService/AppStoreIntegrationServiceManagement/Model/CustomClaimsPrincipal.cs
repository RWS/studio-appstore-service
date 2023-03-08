using AppStoreIntegrationServiceManagement.Model.DataBase;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Security.Principal;

namespace AppStoreIntegrationServiceManagement.Model
{
    public class CustomClaimsPrincipal : ClaimsPrincipal
    {
        private readonly UserManager<IdentityUserExtended> _userManager;
        private readonly UserAccountsManager _userAccountsManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AccountsManager _accountsManager;

        public CustomClaimsPrincipal
        (
            IPrincipal principal,
            UserManager<IdentityUserExtended> userManager, 
            RoleManager<IdentityRole> roleManager,
            UserAccountsManager userAccountsManager,
            AccountsManager accountsManager
        ) : base(principal) 
        {
            _userManager = userManager;
            _userAccountsManager = userAccountsManager;
            _roleManager = roleManager;
            _accountsManager = accountsManager;
        }

        public override bool IsInRole(string roleName)
        {
            var user = _userManager.GetUserAsync(this).Result;

            if (user == null)
            {
                return false;
            }

            var identityRole = _roleManager.FindByNameAsync(roleName).Result;

            if (identityRole == null)
            {
                return false;
            }
            var roleId = identityRole.Id;
            return _userAccountsManager.IsInRole(user, roleId);
        }

        public bool IsInRoles(params string[] roleNames)
        {
            foreach (var roleName in roleNames)
            {
                if (IsInRole(roleName))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsOwner()
        {
            var user = _userManager.GetUserAsync(this).Result;
            return _userAccountsManager.IsOwner(user);
        }

        public bool HasSelectedAccount()
        {
            var user = _userManager.GetUserAsync(this).Result;
            return !string.IsNullOrEmpty(user.SelectedAccountId);
        }

        public string GetRole()
        {
            var user = _userManager.GetUserAsync(this).Result;
            var account = _accountsManager.GetAccountById(user.SelectedAccountId);
            var role = _userAccountsManager.GetUserRoleForAccount(user, account).Result;
            return role.Name;
        }

        public bool HasFullOwnership()
        {
            var user = _userManager.GetUserAsync(this).Result;

            if (user == null)
            {
                return false;
            }

            var identityRole = _roleManager.FindByNameAsync("Administrator").Result;

            if (identityRole == null)
            {
                return false;
            }

            return _userAccountsManager.HasFullOwnership(user, identityRole.Id);
        }
    }
}

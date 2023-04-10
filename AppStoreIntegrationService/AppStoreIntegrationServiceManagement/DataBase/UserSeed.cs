using AppStoreIntegrationServiceCore.DataBase.Interface;
using AppStoreIntegrationServiceCore.DataBase.Models;
using AppStoreIntegrationServiceManagement.DataBase.Interface;
using AppStoreIntegrationServiceManagement.Model.Identity;
using System.Net.Http.Headers;

namespace AppStoreIntegrationServiceManagement.DataBase
{
    public class UserSeed : IUserSeed
    {
        private readonly IUserProfilesManager _userProfilesManager;
        private readonly IAuth0UserManager _auth0UserManager;
        private readonly IUserRolesManager _rolesManager;
        private readonly IUserAccountsManager _userAccountsManager;
        private readonly IAccountsManager _accountsManager;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public UserSeed
        (
            IUserProfilesManager userProfilesManager,
            IUserAccountsManager userAccountsManager,
            IAuth0UserManager auth0UserManager,
            IUserRolesManager rolesManager,
            IAccountsManager accountsManager,
            IConfiguration configuration
        )
        {
            _userAccountsManager = userAccountsManager;
            _userProfilesManager = userProfilesManager;
            _auth0UserManager = auth0UserManager;
            _rolesManager = rolesManager;
            _accountsManager = accountsManager;
            _configuration = configuration;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _configuration["Auth0:APIToken"]);
        }

        public void EnsureAdminExistance()
        {
            if (_userProfilesManager.UserProfiles.Any())
            {
                return;
            }

            InitRoles();

            var model = new RegisterModel
            {
                Username = "Admin",
                Email = "admin@rws.com",
                Password = "Administrator123"
            };
            var user = new UserProfile { Email = model.Email, Id = Guid.NewGuid().ToString() };
            var account = new Account { Name = "AppStore Account", Id = Guid.NewGuid().ToString() };

            _auth0UserManager.TryCreateUser(model).Wait();
            _userProfilesManager.AddUserProfile(user).Wait();
            _accountsManager.TryAddAccount(account).Wait();
            _userAccountsManager.TryAddUserToAccount(new UserAccount
            {
                AccountId = account.Id,
                UserProfileId = user.Id,
                Id = Guid.NewGuid().ToString(),
                UserRoleId = _rolesManager.GetRoleByName("SystemAdministrator").Id
            }).Wait();
        }

        private void InitRoles()
        {
            if (_rolesManager.Roles.Any())
            {
                return;
            }

            var roles = new[] { "SystemAdministrator", "Administrator", "Developer", "DeveloperTrial" };
            foreach (var role in roles)
            {
                _ = _rolesManager.AddRole(new UserRole
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = role
                });
            }
        }
    }
}

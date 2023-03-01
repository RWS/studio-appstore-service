using AppStoreIntegrationServiceManagement.Model.DataBase;
using AppStoreIntegrationServiceManagement.Model.Identity;
using Microsoft.AspNetCore.Identity;

namespace AppStoreIntegrationServiceManagement.Areas.Identity.Data
{
    public class UserSeed : IUserSeed
    {
        private readonly UserManager<IdentityUserExtended> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<IdentityUserExtended> _signInManager;
        private readonly UserAccountsManager _userAccountsManager;

        public UserSeed
        (
            UserManager<IdentityUserExtended> userManager, 
            RoleManager<IdentityRole> roleManager, 
            SignInManager<IdentityUserExtended> signInManager,
            UserAccountsManager userAccountsManager
        )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _userAccountsManager = userAccountsManager;
        }

        public void EnsureAdminExistance()
        {
            if (_userManager.Users.Any())
            {
                return;
            }

            if (!_roleManager.Roles.Any())
            {
                var roles = new[] { "Administrator", "StandardUser", "Developer" };
                for (var i = 0; i < roles.Length; i++)
                {
                    _roleManager.CreateAsync(new IdentityRole
                    {
                        Name = roles[i],
                        Id = $"{i + 1}"
                    }).Wait();
                }
            }

            var defaultAdminUser = new IdentityUserExtended 
            { 
                UserName = "Admin", 
                Email = "admin@sdl.com",
                IsBuiltInAdmin = true 
            };

            _userManager.CreateAsync(defaultAdminUser, "administrator").Wait();
            _userManager.AddToRoleAsync(defaultAdminUser, "Administrator").Wait();
            _userAccountsManager.TryAddUserToAccount(defaultAdminUser).Wait();
            _signInManager.SignInAsync(defaultAdminUser, false).Wait();
        }
    }
}

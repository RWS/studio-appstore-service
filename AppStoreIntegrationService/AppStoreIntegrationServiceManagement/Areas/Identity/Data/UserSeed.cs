using Microsoft.AspNetCore.Identity;

namespace AppStoreIntegrationServiceManagement.Areas.Identity.Data
{
    public class UserSeed : IUserSeed
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public UserSeed(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        public void EnsureAdminExistance()
        {
            if (_userManager.Users.Any())
            {
                return;
            }

            if (!_roleManager.Roles.Any())
            {
                _roleManager.CreateAsync(new IdentityRole
                {
                    Name = "Administrator",
                    Id = "1"
                }).Wait();
                _roleManager.CreateAsync(new IdentityRole
                {
                    Name = "StandardUser",
                    Id = "2"
                }).Wait();
            }

            var defaultAdminUser = new IdentityUser { UserName = "Admin", Email = "admin@sdl.com" };
            _userManager.CreateAsync(defaultAdminUser, "administrator").Wait();
            _userManager.AddToRoleAsync(defaultAdminUser, "Administrator").Wait();
            _signInManager.SignInAsync(defaultAdminUser, false).Wait();
        }
    }
}

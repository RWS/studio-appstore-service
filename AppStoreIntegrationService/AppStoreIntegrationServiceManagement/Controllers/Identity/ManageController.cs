using AppStoreIntegrationServiceManagement.Model.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AppStoreIntegrationServiceManagement.Controllers.Identity
{
    [Area("Identity")]
    [Authorize(Roles = "Administrator")]
    public class ManageController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public ManageController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [Route("[area]/[controller]/Users")]
        public async Task<IActionResult> Index()
        {
            var users = new List<UserInfoModel>();
            var currentUser = await _userManager.GetUserAsync(User);
            foreach(var user in _userManager.Users.ToList())
            {
                if (user.UserName != currentUser.UserName)
                {
                    users.Add(new UserInfoModel
                    {
                        Id = user.Id,
                        Name = user.UserName,
                        Role = (await _userManager.GetRolesAsync(user))[0]
                    });
                }
            }
            
            return View(users);
        }

        [Route("[area]/[controller]/Users/{id?}")]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user.UserName == "Admin")
            {
                return NotFound();
            }

            return View(new UserInfoModel
            {
                Id = id,
                Name = user.UserName,
                Role = (await _userManager.GetRolesAsync(user))[0]
            });
        }

        [HttpPost]
        public async Task<IActionResult> Update(UserInfoModel model, string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            var (newUsername, newRole) = (model.Name, model.Role);

            if (user == null || !ModelState.IsValid)
            {
                TempData["StatusMessage"] = "Error! User is null or model state is invalid!";
                return RedirectToAction("Edit", new { id });
            }

            if (newUsername != user.UserName)
            {
                await _userManager.SetUserNameAsync(user, newUsername);
            }

            var oldRole = (await _userManager.GetRolesAsync(user))[0];
            if (newRole != oldRole)
            {
                await _userManager.RemoveFromRoleAsync(user, oldRole);
                await _userManager.AddToRoleAsync(user, newRole);
            }

            TempData["StatusMessage"] = string.Format("Success! {0} was updated!", user.UserName);
            return RedirectToAction("Edit", new { id });
        }

        [Route("[area]/[controller]/Reset/{id?}")]
        public async Task<IActionResult> ResetPassword(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user.UserName == "Admin")
            {
                return NotFound();
            }

            return View(new ResetPasswordModel
            {
                Id = id,
                Username = user.UserName
            });
        }

        [HttpPost]
        public async Task<IActionResult> PostResetPassword(ResetPasswordModel model, string id)
        {
            var user = await _userManager.FindByIdAsync(model.Id);

            if (user == null && !ModelState.IsValid)
            {
                TempData["StatusMessage"] = "Error! User is null or model state is invalid!";
                return RedirectToAction("ResetPassword", new { id });
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                TempData["StatusMessage"] = "Error! Passwords do not match!";
                return RedirectToAction("ResetPassword", new { id });
            }

            await _userManager.RemovePasswordAsync(user);
            await _userManager.AddPasswordAsync(user, model.NewPassword);
            TempData["StatusMessage"] = "Success! Password was reset!";
            return RedirectToAction("ResetPassword", new { id });
        }

        public async Task<IActionResult> Register()
        {
            return View(new RegisterModel
            {
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            });
        }

        [HttpPost]
        public async Task<IActionResult> PostRegister(RegisterModel registerModel)
        {
            registerModel.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = registerModel.Input.UserName, Email = registerModel.Input.UserName };
                var result = await _userManager.CreateAsync(user, registerModel.Input.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, registerModel.Input.UserRole);
                    TempData["StatusMessage"] = string.Format("Success! {0} was added!", user.UserName);
                    return RedirectToAction("Register");
                }
            }

            TempData["StatusMessage"] = "Error! Something went wrong!";
            return RedirectToAction("Register");
        }

        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null && !ModelState.IsValid)
            {
                TempData["StatusMessage"] = "Error! User is null or model state is invalid!";
                return RedirectToAction("Index");
            }

            await _userManager.DeleteAsync(user);
            return RedirectToAction("Index");
        }
    }
}

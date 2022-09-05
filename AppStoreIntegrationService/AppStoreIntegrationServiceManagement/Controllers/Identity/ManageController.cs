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
        public async Task<IActionResult> Index(string statusMessage)
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
                        Role = (await _userManager.GetRolesAsync(user))[0],
                        StatusMessage = statusMessage
                    });
                }
            }
            
            return View(users);
        }

        [Route("[area]/[controller]/Users/{id?}")]
        public async Task<IActionResult> Edit(string id, string statusMessage)
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
                Role = (await _userManager.GetRolesAsync(user))[0],
                StatusMessage = statusMessage
            });
        }

        [HttpPost]
        public async Task<IActionResult> Update(UserInfoModel model, string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            var (newUsername, newRole) = (model.Name, model.Role);

            if (user == null || !ModelState.IsValid)
            {
                return RedirectToAction("Edit", new { id, statusMessage = "Error! User is null or model state is invalid!" });
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

            return RedirectToAction("Edit", new { id, statusMessage = "Success! User was updated!" });
        }

        [Route("[area]/[controller]/Reset/{id?}")]
        public async Task<IActionResult> ResetPassword(string id, string statusMessage)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user.UserName == "Admin")
            {
                return NotFound();
            }

            return View(new ResetPasswordModel
            {
                Id = id,
                Username = user.UserName,
                StatusMessage = statusMessage
            });
        }

        [HttpPost]
        public async Task<IActionResult> PostResetPassword(ResetPasswordModel model, string id)
        {
            var user = await _userManager.FindByIdAsync(model.Id);

            if (user == null && !ModelState.IsValid)
            {
                return RedirectToAction("ResetPassword", new { id, statusMessage = "Error! User is null or model state is invalid!" });
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                return RedirectToAction("ResetPassword", new { id, statusMessage = "Error! Passwords do not match!" });
            }

            await _userManager.RemovePasswordAsync(user);
            await _userManager.AddPasswordAsync(user, model.NewPassword);

            return RedirectToAction("ResetPassword", new { id, statusMessage = "Success! Password was reset!" });
        }

        public async Task<IActionResult> Register(string statusMessage)
        {
            return View(new RegisterModel
            {
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList(),
                StatusMessage = statusMessage
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
                    return RedirectToAction("Register", new { statusMessage = string.Format("Success! {0} was added!", user.UserName) });
                }
            }

            return View("Register", new RegisterModel { StatusMessage = "Error! Something went wrong!" });
        }

        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null && !ModelState.IsValid)
            {
                return RedirectToAction("Index", new { id, statusMessage = "Error! User is null or model state is invalid!" });
            }

            await _userManager.DeleteAsync(user);
            return RedirectToAction("Index");
        }
    }
}

using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using AppStoreIntegrationServiceManagement.Model;
using AppStoreIntegrationServiceManagement.Model.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceManagement.Controllers.Preservation
{
    public class PreservationController : Controller
    {
        private readonly IPluginRepository _pluginRepository;
        private readonly ICommentsRepository _commentsRepository;
        private readonly ICategoriesRepository _categoriesRepository;
        private readonly IProductsRepository _productsRepository;
        private readonly UserManager<IdentityUser> _userManager;

        public PreservationController
        (
            IPluginRepository pluginRepository, 
            ICommentsRepository commentsRepository, 
            ICategoriesRepository categoriesRepository, 
            IProductsRepository productsRepository,
            UserManager<IdentityUser> userManager
        )
        {
            _pluginRepository = pluginRepository;
            _commentsRepository = commentsRepository;
            _categoriesRepository = categoriesRepository;
            _productsRepository = productsRepository;
            _userManager = userManager;
        }

        [HttpPost("/Preservation/Check")]
        public async Task<IActionResult> Check
        (
            ExtendedPluginVersion version,
            PluginDetails plugin,
            Comment comment,
            CategoryDetails category,
            ParentProduct parent,
            ProductDetails product,
            ProfileModel profile,
            ChangePasswordModel passwordModel,
            RegisterModel registerModel,
            Status status,
            Page page
        )
        {
           return page switch
            {
                Page.Plugin => await Check(plugin, status),
                Page.Version => await Check(version),
                Page.Comment => await Check(comment),
                Page.Categories => await Check(category),
                Page.Products => await Check(product),
                Page.ParentProducts => await Check(parent),
                Page.Profile => await Check(profile),
                Page.Password => Check(passwordModel),
                Page.Register => Check(registerModel),
                _ => Content(null)
            };
        }

        public async Task<IActionResult> Check(ExtendedPluginVersion version)
        {
            var plugin = await _pluginRepository.GetPluginById(version.PluginId, User);
            var saved = plugin?.Versions.FirstOrDefault(v => v.VersionId == version.VersionId);

            if (saved?.Equals(version) ?? true)
            {
                return Content(null);
            }

            return PartialView("_ModalPartial", new ModalMessage
            {
                Message = "Discard changes for this version?"
            });
        }

        public async Task<IActionResult> Check(PluginDetails plugin, Status status)
        {
            var saved = await _pluginRepository.GetPluginById(plugin.Id, User);
            plugin.Status = status;

            if (saved?.Equals(plugin) ?? false)
            {
                return Content(null);
            }

            return PartialView("_ModalPartial", new ModalMessage
            {
                Message = "Discard changes for this plugin?"
            });
        }

        public async Task<IActionResult> Check(Comment comment)
        {
            var plugin = await _pluginRepository.GetPluginById(comment.PluginId);
            var saved = await _commentsRepository.GetComment(plugin.Name, comment.CommentId, comment.VersionId);

            if (saved?.Equals(comment) ?? true)
            {
                return Content(null);
            }

            return PartialView("_ModalPartial", new ModalMessage
            {
                Message = "Discard changes for this comment?"
            });
        }

        public async Task<IActionResult> Check(CategoryDetails category)
        {
            var saved = await _categoriesRepository.GetCategoryById(category.Id);

            if (saved?.Equals(category) ?? true)
            {
                return Content(null);
            }

            return PartialView("_ModalPartial", new ModalMessage
            {
                Message = "Discard changes for this category?"
            });
        }

        public async Task<IActionResult> Check(ParentProduct parent)
        {
            var saved = await _productsRepository.GetParentById(parent.Id);

            if (saved?.Equals(parent) ?? true)
            {
                return Content(null);
            }

            return PartialView("_ModalPartial", new ModalMessage
            {
                Message = "Discard changes for this parent product?"
            });
        }

        public async Task<IActionResult> Check(ProductDetails product)
        {
            var saved = await _productsRepository.GetProductById(product.Id);

            if (saved?.Equals(product) ?? true)
            {
                return Content(null);
            }

            return PartialView("_ModalPartial", new ModalMessage
            {
                Message = "Discard changes for this product?"
            });
        }

        public async Task<IActionResult> Check(ProfileModel profile)
        {
            var editedUser = await _userManager.FindByIdAsync(profile.Id);
            var currentUser = await _userManager.GetUserAsync(User);
            var user = editedUser ?? currentUser;
            var roles = await _userManager.GetRolesAsync(user);

            if (profile?.Equals(new ProfileModel(user, roles[0])) ?? true)
            {
                return Content(null);
            }

            return PartialView("_ModalPartial", new ModalMessage
            {
                Message = "Discard changes for user profile?"
            });
        }

        public IActionResult Check(ChangePasswordModel model)
        {
            if (model.Input.IsEmpty())
            {
                return Content(null);
            }

            return PartialView("_ModalPartial", new ModalMessage
            {
                Message = "Discard changes for the new password?"
            });
        }

        public IActionResult Check(RegisterModel model)
        {
            if (model.Input.IsEmpty())
            {
                return Content(null);
            }

            return PartialView("_ModalPartial", new ModalMessage
            {
                Message = "Discard changes for new user?"
            });
        }
    }
}

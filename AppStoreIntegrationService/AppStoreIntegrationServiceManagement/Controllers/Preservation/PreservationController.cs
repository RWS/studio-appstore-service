using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceManagement.Filters;
using AppStoreIntegrationServiceManagement.Model;
using AppStoreIntegrationServiceManagement.Model.Identity;
using AppStoreIntegrationServiceManagement.Model.Plugins;
using AppStoreIntegrationServiceManagement.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppStoreIntegrationServiceManagement.Controllers.Preservation
{
    public enum Page
    {
        None = 0,
        Details,
        Version,
        Comment,
        Categories,
        ParentProducts,
        Products,
        Register
    }

    [Authorize]
    [AccountSelect]
    public class PreservationController : CustomController
    {
        private readonly IPluginRepository _pluginRepository;
        private readonly IPluginVersionRepository _pluginVersionRepository;
        private readonly ICommentsRepository _commentsRepository;
        private readonly ICategoriesRepository _categoriesRepository;
        private readonly IProductsRepository _productsRepository;

        public PreservationController
        (
            IPluginRepository pluginRepository,
            IPluginVersionRepository pluginVersionRepository,
            ICommentsRepository commentsRepository, 
            ICategoriesRepository categoriesRepository, 
            IProductsRepository productsRepository
        )
        {
            _pluginRepository = pluginRepository;
            _commentsRepository = commentsRepository;
            _categoriesRepository = categoriesRepository;
            _productsRepository = productsRepository;
            _pluginVersionRepository = pluginVersionRepository;
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
            RegisterModel registerModel,
            Status status,
            Page page
        )
        {
           return page switch
            {
                Page.Details => await Check(plugin, status),
                Page.Version => await Check(version),
                Page.Comment => await Check(comment),
                Page.Categories => await Check(category),
                Page.Products => await Check(product),
                Page.ParentProducts => await Check(parent),
                Page.Register => Check(registerModel),
                _ => Content(null)
            };
        }

        public async Task<IActionResult> Check(ExtendedPluginVersion version)
        {
            var saved = await _pluginVersionRepository.GetPluginVersion(version.PluginId, version.VersionId, status: version.VersionStatus);

            if (saved?.Equals(version) ?? false)
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
            var saved = await _pluginRepository.GetPluginById(plugin.Id, status: status);
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
            var saved = await _commentsRepository.GetComment(comment.PluginId, comment.CommentId, comment.VersionId);

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

        public IActionResult Check(RegisterModel model)
        {
            if (model.IsEmpty())
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

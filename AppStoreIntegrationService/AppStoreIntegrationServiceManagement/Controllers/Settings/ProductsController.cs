using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Common.Interface;
using AppStoreIntegrationServiceCore.Repository.Interface;
using AppStoreIntegrationServiceManagement.Model;
using AppStoreIntegrationServiceManagement.Model.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AppStoreIntegrationServiceManagement.Controllers.Settings
{
    [Area("Settings")]
    [Authorize(Policy = "IsAdmin")]
    public class ProductsController : Controller
    {
        private readonly IProductsRepository _productsRepository;
        private readonly IProductsSynchronizer _productsSynchronizer;

        public ProductsController(IProductsRepository productsRepository, IProductsSynchronizer productsSynchronizer)
        {
            _productsRepository = productsRepository;
            _productsSynchronizer = productsSynchronizer;
        }

        public async Task<IActionResult> Index()
        {
            return View(new ProductsModel
            {
                Products = await _productsRepository.GetAllProducts()
            });
        }

        [HttpPost]
        public IActionResult AddNew()
        {
            return PartialView("_NewProductPartial", new ProductDetails());
        }

        [HttpPost]
        public async Task<IActionResult> Add(ProductDetails product, List<ProductDetails> products)
        {
            if (TryValidateProduct(product, products, out IActionResult result))
            {
                products.Add(product);
                await _productsRepository.UpdateProducts(products);
            }

            return result;
        }

        [HttpPost]
        public async Task<IActionResult> Update(List<ProductDetails> products)
        {
            if (!products.Any(item => item.IsValid()))
            {
                return PartialView("_StatusMessage", "Error! Parameter cannot be null!");
            }

            if (ExistVersion(products))
            {
                return PartialView("_StatusMessage", "Error! There is already a product with this version!");
            }

            await _productsRepository.UpdateProducts(products);
            await _productsSynchronizer.SyncOnUpdate(products);
            TempData["StatusMessage"] = "Success! Products were updated and synchronized with plugins!";
            return Content("/Settings/Products");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (await _productsSynchronizer.IsInUse(id))
            {
                TempData["StatusMessage"] = "Error! This product is used by plugins!";
                return Content("/Settings/Products");
            }

            await _productsRepository.DeleteProduct(id);
            TempData["StatusMessage"] = "Success! Product was deleted!";
            return Content("/Settings/Products");
        }

        [Route("[controller]/[action]/{redirectUrl?}")]
        [HttpPost]
        public async Task<IActionResult> GoToPage(string redirectUrl, ProductDetails product, List<ProductDetails> products)
        {
            redirectUrl = redirectUrl.Replace('.', '/');

            if (string.IsNullOrEmpty(product.ProductName) &&
                string.IsNullOrEmpty(product.MinimumStudioVersion) &&
                await HaveUnsavedChanges(products))
            {
                return Content(redirectUrl);
            }

            var modalDetails = new ModalMessage
            {
                ModalType = ModalType.WarningMessage,
                Title = "Warning!",
                Message = $"Discard changes for product?",
                RequestPage = $"{redirectUrl}"
            };

            return PartialView("_ModalPartial", modalDetails);
        }

        public bool ExistVersion(List<ProductDetails> products)
        {
            return !products.GroupBy(p => p.ProductName, (_, products) => products
                            .GroupBy(p => p.MinimumStudioVersion, (_, products) => products
                            .Count() == 1).All(item => item))
                            .All(item => item);
        }

        private async Task<bool> HaveUnsavedChanges(List<ProductDetails> products)
        {
            var savedNamesMapping = (await _productsRepository.GetAllProducts()).ToList();
            return JsonConvert.SerializeObject(savedNamesMapping) == JsonConvert.SerializeObject(products);
        }

        private bool TryValidateProduct(ProductDetails product, List<ProductDetails> products, out IActionResult result)
        {
            if (string.IsNullOrEmpty(product.ProductName) ||
                string.IsNullOrEmpty(product.Id))
            {
                result = PartialView("_StatusMessage", "Error! Parameter cannot be null!");
                return false;
            }

            if (products.Any(p => p.Id == product.Id || p.Equals(product)))
            {
                result = PartialView("_StatusMessage", "Error! There are duplicated params!");
                return false;
            }

            TempData["StatusMessage"] = "Success! Product was added!";
            result = Content("/Settings/Products");
            return true;
        }
    }
}

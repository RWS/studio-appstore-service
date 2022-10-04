using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
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

        public ProductsController(IProductsRepository productsRepository)
        {
            _productsRepository = productsRepository;
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
            return PartialView("_NewProductPartial", new SupportedProductDetails());
        }

        [HttpPost]
        public async Task<IActionResult> Add(SupportedProductDetails product, List<SupportedProductDetails> products)
        {
            if (TryValidateProduct(product, products, out IActionResult result))
            {
                products.Add(product);
                await _productsRepository.UpdateProducts(products);
            }

            return result;
        }

        [HttpPost]
        public async Task<IActionResult> Update(List<SupportedProductDetails> products)
        {
            if (!products.Any(item => string.IsNullOrEmpty(item.ProductName)))
            {
                await _productsRepository.UpdateProducts(products);
                TempData["StatusMessage"] = "Success! Product was updated!";
                return Content("/Settings/Products");
            }

            return PartialView("_StatusMessage", "Error! Parameter cannot be null!");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            await _productsRepository.DeleteProduct(id);
            TempData["StatusMessage"] = "Success! Product was deleted!";
            return Content("/Settings/Products");
        }

        [Route("[controller]/[action]/{redirectUrl?}")]
        [HttpPost]
        public async Task<IActionResult> GoToPage(string redirectUrl, SupportedProductDetails product, List<SupportedProductDetails> products)
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

        private async Task<bool> HaveUnsavedChanges(List<SupportedProductDetails> products)
        {
            var savedNamesMapping = (await _productsRepository.GetAllProducts()).ToList();
            return JsonConvert.SerializeObject(savedNamesMapping) == JsonConvert.SerializeObject(products);
        }

        private bool TryValidateProduct(SupportedProductDetails product, List<SupportedProductDetails> products, out IActionResult result)
        {
            if (string.IsNullOrEmpty(product.ProductName) ||
                string.IsNullOrEmpty(product.Id))
            {
                result = PartialView("_StatusMessage", "Error! Parameter cannot be null!");
                return false;
            }

            if (products.Any(p => p.Id == product.Id || p.ProductName == product.ProductName))
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

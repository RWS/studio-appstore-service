using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.V2;
using AppStoreIntegrationServiceCore.Repository.V2.Interface;
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
            var products = await _productsRepository.GetAllProducts();
            var parents = await _productsRepository.GetAllParents();
            var extendedProducts = new List<ExtendedProductDetails>();

            foreach (var product in products)
            {
                var extended = new ExtendedProductDetails(product);
                extended.SetParentProductsList(parents);
                extendedProducts.Add(extended);
            }

            return View(new ProductsModel
            {
                Products = extendedProducts,
                ParentProducts = parents
            });
        }

        [HttpPost]
        public async Task<IActionResult> AddNew()
        {
            var parents = await _productsRepository.GetAllParents();
            var products = await _productsRepository.GetAllProducts();
            var product = new ExtendedProductDetails
            {
                Id = _productsSynchronizer.SetIndex(products)
            };
            product.SetParentProductsList(parents);
            return PartialView("_NewProductPartial", product);
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

            if (_productsSynchronizer.ExistDuplicate(products))
            {
                return PartialView("_StatusMessage", "Error! There is already a product with this version!");
            }

            await _productsRepository.UpdateProducts(products);
            TempData["StatusMessage"] = "Success! Products were updated!";
            return Content("/Settings/Products");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (await _productsSynchronizer.IsInUse(id, ProductType.Child))
            {
                TempData["StatusMessage"] = "Error! This product is used by plugins!";
                return Content("/Settings/Products");
            }

            await _productsRepository.DeleteProduct(id, ProductType.Child);
            TempData["StatusMessage"] = "Success! Product was deleted!";
            return Content("/Settings/Products");
        }

        [Route("[controller]/[action]/{redirectUrl?}")]
        [HttpPost]
        public async Task<IActionResult> GoToPage(string redirectUrl, ProductDetails product, List<ProductDetails> products, ParentProduct parentProduct, List<ParentProduct> parentProducts)
        {
            redirectUrl = redirectUrl.Replace('.', '/');

            if (string.IsNullOrEmpty(product.ProductName) &&
                string.IsNullOrEmpty(product.MinimumStudioVersion) &&
                string.IsNullOrEmpty(parentProduct.ParentProductName) &&
                await AreSavedProducts(products, parentProducts))
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

        private async Task<bool> AreSavedProducts(List<ProductDetails> products, List<ParentProduct> parentProducts)
        {
            var savedProducts = (await _productsRepository.GetAllProducts()).ToList();
            var savedParentProducts = (await _productsRepository.GetAllParents()).ToList();
            return JsonConvert.SerializeObject(savedProducts) == JsonConvert.SerializeObject(products) &&
                   JsonConvert.SerializeObject(savedParentProducts) == JsonConvert.SerializeObject(parentProducts);
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

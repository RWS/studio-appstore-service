using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceManagement.Controllers.Settings
{
    [Area("Settings")]
    [Authorize(Policy = "IsAdmin")]
    public class ProductsController : Controller
    {
        private readonly IProductsRepository _productsRepository;
        private readonly IPluginManager _pluginManager;

        public ProductsController(IProductsRepository productsRepository, IPluginManager pluginManager)
        {
            _productsRepository = productsRepository;
            _pluginManager = pluginManager;
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

            return View(extendedProducts);
        }

        [HttpPost]
        public async Task<IActionResult> Add()
        {
            var parents = await _productsRepository.GetAllParents();
            var products = await _productsRepository.GetAllProducts();
            var product = new ExtendedProductDetails
            {
                Id = SetIndex(products)
            };
            product.SetParentProductsList(parents);
            return PartialView("_NewProductPartial", product);
        }

        [HttpPost]
        public async Task<IActionResult> Update(ProductDetails product)
        {
            if (await _productsRepository.TryUpdateProduct(product))
            {
                TempData["StatusMessage"] = "Success! Products were updated!";
                return Content("/Settings/Products");
            }

            return PartialView("_StatusMessage", "Error! There is already a product with this name!");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var inUse = (await _pluginManager.GetPlugins()).SelectMany(p => p.Versions.SelectMany(v => v.SupportedProducts));
            if (inUse.Any(p => p.Equals(id)))
            {
                TempData["StatusMessage"] = "Error! This product is used by plugins!";
                return Content("/Settings/Products");
            }

            await _productsRepository.DeleteProduct(id, ProductType.Child);
            TempData["StatusMessage"] = "Success! Product was deleted!";
            return Content("/Settings/Products");
        }

        public string SetIndex(IEnumerable<ProductDetails> products)
        {
            var lastProduct = products.LastOrDefault();
            if (lastProduct == null)
            {
                return "1";
            }

            return (int.Parse(lastProduct.Id) + 1).ToString();
        }
    }
}

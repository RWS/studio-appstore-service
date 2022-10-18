using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Common;
using AppStoreIntegrationServiceCore.Repository.Common.Interface;
using AppStoreIntegrationServiceCore.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppStoreIntegrationServiceManagement.Controllers.Settings
{
    [Area("Settings")]
    [Authorize(Policy = "IsAdmin")]
    public class ParentProductsController : Controller
    {
        private readonly IProductsRepository _productsRepository;
        private readonly IProductsSynchronizer _productsSynchronizer;

        public ParentProductsController(IProductsRepository productsRepository, IProductsSynchronizer productsSynchronizer)
        {
            _productsRepository = productsRepository;
            _productsSynchronizer = productsSynchronizer;
        }

        [HttpPost]
        public async Task<IActionResult> AddNew()
        {
            var products = await _productsRepository.GetAllParentProducts();
            return PartialView("_NewParentProductPartial", new ParentProduct
            {
                ParentId = SetIndex(products)
            });
        }

        [HttpPost]
        public async Task<IActionResult> Add(ParentProduct product, List<ParentProduct> products)
        {
            if (TryValidateProduct(product, products, out IActionResult result))
            {
                products.Add(product);
                await _productsRepository.UpdateParentProducts(products);
            }

            return result;
        }

        [HttpPost]
        public async Task<IActionResult> Update(List<ParentProduct> products)
        {
            if (!products.Any(item => item.IsValid()))
            {
                return PartialView("_StatusMessage", "Error! Parameter cannot be null!");
            }

            if (ExitsProduct(products))
            {
                return PartialView("_StatusMessage", "Error! There is already a parent product with this name!");
            }

            await _productsRepository.UpdateParentProducts(products);
            TempData["StatusMessage"] = "Success! Parent products table was updated!";
            return Content("/Settings/Products");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (await _productsSynchronizer.IsInUse(id, ProductType.Parent))
            {
                TempData["StatusMessage"] = "Error! This parent product is used among child products!";
                return Content("/Settings/Products");
            }

            await _productsRepository.DeleteParentProduct(id);
            TempData["StatusMessage"] = "Success! Product was deleted!";
            return Content("/Settings/Products");
        }

        private static string SetIndex(IEnumerable<ParentProduct> products)
        {
            var lastProduct = products.LastOrDefault();
            if (lastProduct == null)
            {
                return "1";
            }

            return (int.Parse(lastProduct.ParentId) + 1).ToString();
        }

        public bool ExitsProduct(List<ParentProduct> products)
        {
            return !products.GroupBy(p => p.ParentProductName, (_, products) => products
                            .Count() == 1)
                            .All(item => item);
        }

        private bool TryValidateProduct(ParentProduct product, List<ParentProduct> products, out IActionResult result)
        {
            if (string.IsNullOrEmpty(product.ParentProductName) ||
                string.IsNullOrEmpty(product.ParentId))
            {
                result = PartialView("_StatusMessage", "Error! Parameter cannot be null!");
                return false;
            }

            if (products.Any(p => p.ParentId == product.ParentId || p.Equals(product)))
            {
                result = PartialView("_StatusMessage", "Error! There are duplicated params!");
                return false;
            }

            TempData["StatusMessage"] = "Success! Parent product was added!";
            result = Content("/Settings/Products");
            return true;
        }
    }
}

using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceManagement.Controllers.Settings
{
    [Area("Settings")]
    [Authorize(Policy = "IsAdmin")]
    public class ParentProductsController : Controller
    {
        private readonly IProductsRepository _productsRepository;

        public ParentProductsController(IProductsRepository productsRepository)
        {
            _productsRepository = productsRepository;
        }

        [HttpPost]
        public async Task<IActionResult> AddNew()
        {
            var products = await _productsRepository.GetAllParents();
            return PartialView("_NewParentProductPartial", new ParentProduct
            {
                ParentId = SetIndex(products)
            });
        }

        [HttpPost]
        public async Task<IActionResult> Update(List<ParentProduct> products)
        {
            await _productsRepository.UpdateProducts(products);
            TempData["StatusMessage"] = "Success! Parent products table was updated!";
            return Content("/Settings/Products");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var products = await _productsRepository.GetAllProducts();
            if (products.Any(p => p.ParentProductID.Equals(id)))
            {
                TempData["StatusMessage"] = "Error! This parent product is used among child products!";
                return Content("/Settings/Products");
            }

            await _productsRepository.DeleteProduct(id, ProductType.Parent);
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
    }
}

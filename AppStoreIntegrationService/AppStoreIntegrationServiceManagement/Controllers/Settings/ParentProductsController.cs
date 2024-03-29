﻿using AppStoreIntegrationServiceCore.Model;
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

        public ParentProductsController(IProductsRepository productsRepository)
        {
            _productsRepository = productsRepository;
        }

        [Route("/Settings/Products/Parents")]
        public async Task<IActionResult> Index()
        {
            return View(await _productsRepository.GetAllParents());
        }

        [HttpPost]
        public async Task<IActionResult> Add()
        {
            var products = await _productsRepository.GetAllParents();
            return PartialView("_NewParentProductPartial", new ParentProduct
            {
                Id = SetIndex(products)
            });
        }

        [HttpPost]
        public async Task<IActionResult> Update(ParentProduct product)
        {
            if (await _productsRepository.TryUpdateProduct(product))
            {
                TempData["StatusMessage"] = "Success! Parent products table was updated!";
                return Content(null);
            }

            return PartialView("_StatusMessage", "Error! There is already a parent product with this name!");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var products = await _productsRepository.GetAllProducts();
            if (products.Any(p => p.ParentProductID.Equals(id)))
            {
                TempData["StatusMessage"] = "Error! This parent product is used among child products!";
                return Content(null);
            }

            await _productsRepository.DeleteParent(id);
            TempData["StatusMessage"] = "Success! Paret product was deleted!";
            return Content(null);
        }

        private static string SetIndex(IEnumerable<ParentProduct> products)
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

using System.Net.Mime;
using AppStoreIntegrationServiceCore.Repository.Interface;
using Microsoft.AspNetCore.Mvc;

namespace AppStoreIntegrationServiceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces(MediaTypeNames.Application.Json)]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoriesRepository _categoriesRepository;

        public CategoriesController(ICategoriesRepository categoriesRepository)
        {
            _categoriesRepository = categoriesRepository;
        }

        public async Task<IActionResult> Get()
        {
            var categories = await _categoriesRepository.GetAllCategories();
            return Ok(categories);
        }
    }
}
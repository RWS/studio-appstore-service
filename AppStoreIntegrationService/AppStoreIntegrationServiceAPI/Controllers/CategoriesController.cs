using System.Net.Mime;
using AppStoreIntegrationServiceCore.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppStoreIntegrationServiceAPI.Controllers
{

    [ApiController]
    [Produces(MediaTypeNames.Application.Json)]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoriesRepositoryReadonly _categoriesRepository;

        public CategoriesController(ICategoriesRepositoryReadonly categoriesRepository)
        {
            _categoriesRepository = categoriesRepository;
        }

        [ResponseCache(Location = ResponseCacheLocation.Any, NoStore = true, VaryByQueryKeys = new[] { "*" })]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("api/[controller]")]
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var categories = await _categoriesRepository.GetAllCategories();
            return Ok(categories);
        }
    }
}
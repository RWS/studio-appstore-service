using System.Net.Mime;
using AppStoreIntegrationServiceCore.Repository;
using Microsoft.AspNetCore.Mvc;

namespace AppStoreIntegrationServiceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces(MediaTypeNames.Application.Json)]
    public class CategoriesController : ControllerBase
    {
        private readonly IPluginRepository _pluginRepository;

        public CategoriesController(IPluginRepository pluginRepository)
        {
            _pluginRepository = pluginRepository;
        }

        public async Task<IActionResult> Get()
        {
            var categories = await _pluginRepository.GetCategories();
            return Ok(categories);
        }
    }
}
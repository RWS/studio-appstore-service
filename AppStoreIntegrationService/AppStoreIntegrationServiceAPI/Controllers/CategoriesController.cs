using System.Net.Mime;
using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.V2.Interface;
using Microsoft.AspNetCore.Mvc;

namespace AppStoreIntegrationServiceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces(MediaTypeNames.Application.Json)]
    public class CategoriesController : ControllerBase
    {
        private readonly IPluginRepositoryExtended<PluginDetails<PluginVersion<string>>> _pluginRepository;

        public CategoriesController(IPluginRepositoryExtended<PluginDetails<PluginVersion<string>>> pluginRepository)
        {
            _pluginRepository = pluginRepository;
        }

        public IActionResult Get()
        {
            var categories = _pluginRepository.GetCategories();
            return Ok(categories);
        }
    }
}
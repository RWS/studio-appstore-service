using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Authorization;
using AppStoreIntegrationServiceCore.Repository.V1.Interface;
using AppStoreIntegrationServiceCore.Repository.V2.Interface;
using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceAPI.Controllers
{
    [ApiController]
	[Route("[controller]")]
	[Route("")]
	[Produces(MediaTypeNames.Application.Json)]
	public class PluginsController : Controller
	{
		public IPluginRepository<PluginDetails<PluginVersion<ProductDetails>>> _pluginRepository;
		public IPluginRepositoryExtended<PluginDetails<PluginVersion<string>>> _pluginRepositoryExtended;

		public PluginsController
		(
			IPluginRepository<PluginDetails<PluginVersion<ProductDetails>>> pluginRepository, 
			IPluginRepositoryExtended<PluginDetails<PluginVersion<string>>> pluginRepositoryExtended
		)
		{
			_pluginRepository = pluginRepository;
			_pluginRepositoryExtended = pluginRepositoryExtended;
		}

		[ProducesResponseType(StatusCodes.Status200OK)]
		[AllowAnonymous]
		[ResponseCache(Duration = 540, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "*" })]
		public async Task<IActionResult> Get([FromQuery] PluginFilter filter)
		{
			_ = Request.Headers.TryGetValue("apiversion", out StringValues version);
            filter.SortOrder = string.IsNullOrEmpty(filter?.SortOrder) ? "asc" : filter.SortOrder;
            if (string.IsNullOrEmpty(version) || version == "1.0.0")
			{
                return Ok(_pluginRepository.SearchPlugins(await _pluginRepository.GetAll(filter.SortOrder), filter));
            }

            return Ok(_pluginRepositoryExtended.SearchPlugins(await _pluginRepositoryExtended.GetAll(filter.SortOrder), filter));
        }
	}
}
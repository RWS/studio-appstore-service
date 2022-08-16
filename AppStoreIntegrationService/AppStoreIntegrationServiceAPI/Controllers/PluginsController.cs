using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;

namespace AppStoreIntegrationServiceAPI.Controllers
{
    [ApiController]
	[Route("[controller]")]
	[Route("")]
	[Produces(MediaTypeNames.Application.Json)]
	[Authorize]
	public class PluginsController : Controller
	{
		public IPluginRepository PluginRepository { get; set; }

		private readonly IHttpContextAccessor _contextAccessor;

		public PluginsController(IPluginRepository pluginRepository, IHttpContextAccessor contextAccessor)
		{
			PluginRepository = pluginRepository;
			_contextAccessor = contextAccessor;
		}

		[ProducesResponseType(StatusCodes.Status200OK)]
		[AllowAnonymous]
		[ResponseCache(Duration = 540, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "*" })]
		public async Task<IActionResult> Get([FromQuery] PluginFilter filter)
		{
			filter.SortOrder = string.IsNullOrEmpty(filter?.SortOrder) ? "asc" : filter.SortOrder;
			List<PluginDetails> pluginsList = await PluginRepository.GetAll(filter.SortOrder);
			return Ok(PluginRepository.SearchPlugins(pluginsList, filter));
		}
	}
}
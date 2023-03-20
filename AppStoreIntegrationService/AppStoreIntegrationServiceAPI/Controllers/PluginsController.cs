using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Authorization;
using AppStoreIntegrationServiceAPI.Model.Repository.Interface;
using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using AppStoreIntegrationServiceManagement.Repository.Interface;
using AppStoreIntegrationServiceAPI.Filters;

namespace AppStoreIntegrationServiceAPI.Controllers
{
    [ApiController]
    [Produces(MediaTypeNames.Application.Json)]
    public class PluginsController : Controller
    {
        private readonly IPluginResponseConverter _converter;
        private readonly IResponseManager _manager;
        private readonly IPluginRepository _pluginRepository;

        public PluginsController
        (
            IPluginResponseConverter converter, 
            IResponseManager manager,
            IPluginRepository pluginRepository
        )
        {
            _converter = converter;
            _manager = manager;
            _pluginRepository = pluginRepository;
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [AllowAnonymous]
        [Route("/")]
        public IActionResult Index()
        {
            return Ok();
        }

        [ResponseCache(Location = ResponseCacheLocation.Any, NoStore = true, VaryByQueryKeys = new[] { "*" })]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [AllowAnonymous]
        [Route("Plugins")]
        public async Task<IActionResult> Index([FromQuery] PluginFilter filter)
        {
            _ = Request.Headers.TryGetValue("apiversion", out StringValues text);
            filter.SortOrder = string.IsNullOrEmpty(filter?.SortOrder) ? "asc" : filter.SortOrder;
            var response = await _manager.GetBaseResponse();
            response.Value = PluginFilter.FilterPlugins(response.Value, filter, response.Products, response.ParentProducts);

            if (!Version.TryParse(text, out Version version) || version == new Version(1, 0, 0))
            {
                return Ok(_converter.CreateOldResponse(response));
            }

            if (version == new Version(2, 0, 0))
            {
                return Ok(_converter.CreateBaseResponse(response));
            }

            return NotFound();
        }
        
        [HttpPost("/Increment")]
        [TokenAuthorization]
        public async Task<IActionResult> Increment([FromQuery] string pluginName)
        {
            return Ok(await _pluginRepository.TryIncrementDownloadCount(pluginName));
        }
    }
}
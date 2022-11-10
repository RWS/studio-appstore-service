using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Authorization;
using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceAPI.Model.Repository.Interface;
using AppStoreIntegrationServiceAPI.Model;

namespace AppStoreIntegrationServiceAPI.Controllers
{
    [ApiController]
    [Produces(MediaTypeNames.Application.Json)]
    public class PluginsController : Controller
    {
        private readonly IPluginResponseConverter<PluginDetails<PluginVersion<string>, string>, PluginDetails<PluginVersion<ProductDetails>, CategoryDetails>> _converter;
        private readonly IResponseRepository<PluginDetails<PluginVersion<string>, string>> _responseRepository;

        public PluginsController
        (
            IPluginResponseConverter<PluginDetails<PluginVersion<string>, string>, PluginDetails<PluginVersion<ProductDetails>, CategoryDetails>> converter,
            IResponseRepository<PluginDetails<PluginVersion<string>, string>> responseRepository
        )
        {
            _converter = converter;
            _responseRepository = responseRepository;
        }

        [ResponseCache(Duration = 540, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "*" })]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [AllowAnonymous]
        [Route("Plugins")]
        [Route("/")]
        public async Task<IActionResult> Index([FromQuery] PluginFilter filter)
        {
            _ = Request.Headers.TryGetValue("apiversion", out StringValues text);
            filter.SortOrder = string.IsNullOrEmpty(filter?.SortOrder) ? "asc" : filter.SortOrder;
            var response = await _responseRepository.GetResponse();

            if (!APIVersion.TryParse(text, out APIVersion version) || version.IsVersion(1, 0, 0))
            {
                return Ok(_converter.CreateOldResponse(response).Value);
            }

            if (version.IsVersion(2, 0, 0))
            {
                return Ok(response);
            }

            return NotFound();
        }
    }
}
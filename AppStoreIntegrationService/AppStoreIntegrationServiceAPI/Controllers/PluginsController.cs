using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Authorization;
using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceAPI.Model.Repository.Interface;
using AppStoreIntegrationServiceAPI.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;

namespace AppStoreIntegrationServiceAPI.Controllers
{
    [ApiController]
    [Produces(MediaTypeNames.Application.Json)]
    public class PluginsController : Controller
    {
        private readonly IPluginResponseConverter<PluginDetails<PluginVersion<string>, string>, PluginDetails<PluginVersion<ProductDetails>, CategoryDetails>> _converter;
        private readonly IResponseRepository<PluginDetails<PluginVersion<string>, string>> _responseRepository;
        private readonly IPluginRepository<PluginDetails<PluginVersion<string>, string>> _pluginRepository;
        private readonly IProductsRepository _productsRepository;

        public PluginsController
        (
            IPluginResponseConverter<PluginDetails<PluginVersion<string>, string>, PluginDetails<PluginVersion<ProductDetails>, CategoryDetails>> converter,
            IResponseRepository<PluginDetails<PluginVersion<string>, string>> responseRepository,
            IPluginRepository<PluginDetails<PluginVersion<string>, string>> pluginRepository,
            IProductsRepository productsRepository
        )
        {
            _converter = converter;
            _responseRepository = responseRepository;
            _pluginRepository = pluginRepository;
            _productsRepository = productsRepository;
        }

        [ResponseCache(Location = ResponseCacheLocation.Any, NoStore = true, VaryByQueryKeys = new[] { "*" })]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [AllowAnonymous]
        [Route("Plugins")]
        [Route("/")]
        public async Task<IActionResult> Index([FromQuery] PluginFilter filter)
        {
            _ = Request.Headers.TryGetValue("apiversion", out StringValues text);
            filter.SortOrder = string.IsNullOrEmpty(filter?.SortOrder) ? "asc" : filter.SortOrder;
            var response = await _responseRepository.GetResponse();
            var products = await _productsRepository.GetAllProducts();
            response.Value = _pluginRepository.SearchPlugins(response.Value, filter, products);

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
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Authorization;
using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceAPI.Model;
using ResponseConverter.ViewModel;
using AppStoreIntegrationServiceCore.Repository.Interface;

namespace AppStoreIntegrationServiceAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Route("")]
    [Produces(MediaTypeNames.Application.Json)]
    public class PluginsController : Controller
    {
        private readonly IProductsRepository _productsRepository;
        private readonly IVersionProvider _versionProvider;
        private readonly IPluginRepository<PluginDetails<PluginVersion<string>, string>> _pluginRepository;
        private readonly ICategoriesRepository _categoriesRepository;
        private PluginResponseConverter<PluginDetails<PluginVersion<string>, string>, PluginDetails<PluginVersion<ProductDetails>, CategoryDetails>> converter;

        public PluginsController
        (
            IPluginRepository<PluginDetails<PluginVersion<string>, string>> pluginRepository,
            IProductsRepository productsRepository,
            IVersionProvider versionProvider,
            ICategoriesRepository categoriesRepository
        )
        {
            _pluginRepository = pluginRepository;
            _productsRepository = productsRepository;
            _versionProvider = versionProvider;
            _categoriesRepository = categoriesRepository;
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [AllowAnonymous]
        [ResponseCache(Duration = 540, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "*" })]
        public async Task<IActionResult> Get([FromQuery] PluginFilter filter)
        {
            _ = Request.Headers.TryGetValue("apiversion", out StringValues text);
            filter.SortOrder = string.IsNullOrEmpty(filter?.SortOrder) ? "asc" : filter.SortOrder;
            var response = await InitResponse(filter);

            if (!APIVersion.TryParse(text, out APIVersion version) || version.IsVersion(1, 0, 0))
            {
                converter = new(response);
                return Ok(converter.CreateOldResponse().Value);
            }

            if (version.IsVersion(2, 0, 0))
            {
                return Ok(response);
            }

            return NotFound();
        }

        private async Task<PluginResponse<PluginDetails<PluginVersion<string>, string>>> InitResponse(PluginFilter filter)
        {
            return new PluginResponse<PluginDetails<PluginVersion<string>, string>>
            {
                APIVersion = await _versionProvider.GetAPIVersion(),
                Value = _pluginRepository.SearchPlugins(await _pluginRepository.GetAll(filter.SortOrder), filter),
                Products = await _productsRepository.GetAllProducts(),
                ParentProducts = await _productsRepository.GetAllParents(),
                Categories = await _categoriesRepository.GetAllCategories()
            };
        }
    }
}
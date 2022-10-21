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
		private readonly IPluginRepository<PluginDetails<PluginVersion<ProductDetails>, CategoryDetails>> _pluginRepository;
		private readonly IProductsRepository _productsRepository;
		private readonly IVersionProvider _versionProvider;
		private readonly IPluginRepositoryExtended<PluginDetails<PluginVersion<string>, string>> _pluginRepositoryExtended;
		private readonly ICategoriesRepository _categoriesRepository;

		public PluginsController
		(
			IPluginRepository<PluginDetails<PluginVersion<ProductDetails>, CategoryDetails>> pluginRepository, 
			IPluginRepositoryExtended<PluginDetails<PluginVersion<string>, string>> pluginRepositoryExtended,
			IProductsRepository productsRepository,
			IVersionProvider versionProvider,
			ICategoriesRepository categoriesRepository
		)
		{
			_pluginRepository = pluginRepository;
			_pluginRepositoryExtended = pluginRepositoryExtended;
			_productsRepository = productsRepository;
			_versionProvider = versionProvider;
			_categoriesRepository = categoriesRepository;
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

            return Ok(new PluginResponse<PluginDetails<PluginVersion<string>, string>>
            {
                APIVersion = await _versionProvider.GetAPIVersion(),
                Value = _pluginRepositoryExtended.SearchPlugins(await _pluginRepositoryExtended.GetAll(filter.SortOrder), filter),
				Products = await _productsRepository.GetAllProducts(),
				ParentProducts = await _productsRepository.GetAllParents(),
				Categories = await _categoriesRepository.GetAllCategories()
            });
        }
	}
}
﻿using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Authorization;
using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceAPI.Model.Repository.Interface;
using AppStoreIntegrationServiceCore.Repository.Interface;

namespace AppStoreIntegrationServiceAPI.Controllers
{
    [ApiController]
    [Produces(MediaTypeNames.Application.Json)]
    public class PluginsController : Controller
    {
        private readonly IPluginResponseConverter _converter;
        private readonly IResponseRepository _responseRepository;
        private readonly IPluginRepository _pluginRepository;

        public PluginsController
        (
            IPluginResponseConverter converter,
            IResponseRepository responseRepository,
            IPluginRepository pluginRepository
        )
        {
            _converter = converter;
            _responseRepository = responseRepository;
            _pluginRepository = pluginRepository;
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
            response.Value = _pluginRepository.SearchPlugins(response.Value, filter, response.Products);

            if (!Version.TryParse(text, out Version version) || version == new Version(1, 0, 0))
            {
                return Ok(_converter.CreateOldResponse(response).Value);
            }

            if (version == new Version(1, 0, 0))
            {
                return Ok(response);
            }

            return NotFound();
        }
    }
}
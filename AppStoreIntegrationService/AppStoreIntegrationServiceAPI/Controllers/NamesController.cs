using System.Net.Mime;
using AppStoreIntegrationServiceAPI.Model;
using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using Microsoft.AspNetCore.Mvc;

namespace AppStoreIntegrationServiceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces(MediaTypeNames.Application.Json)]
    public class NamesController : ControllerBase
    {
        private readonly INamesRepositoryReadonly _namesRepository;

        public NamesController(INamesRepositoryReadonly namesRepository)
        {
            _namesRepository = namesRepository;
        }

        [HttpGet("/mapNames")]
        public async Task<IActionResult> Get([FromQuery] PluginNamesRequest pluginNamesRequest)
        {
            if (!(bool)pluginNamesRequest.Name?.Any())
            {
                return Ok(new List<NameMapping>());
            }

            var nameMappings = await _namesRepository.GetAllNames(pluginNamesRequest.Name);
            return Ok(nameMappings);
        }
    }
}
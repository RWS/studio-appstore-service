using System.Net.Mime;
using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository;
using Microsoft.AspNetCore.Mvc;

namespace AppStoreIntegrationServiceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces(MediaTypeNames.Application.Json)]
    public class NamesController : ControllerBase
    {
        private readonly INamesRepository _namesRepository;

        public NamesController(INamesRepository namesRepository)
        {
            _namesRepository = namesRepository;
        }

        [HttpGet("/mapNames")]
        public async Task<IActionResult> Get([FromQuery] PluginNamesRequest pluginNamesRequest)
        {
            if (!(pluginNamesRequest?.Name?.Count > 0))
            {
                return Ok(new List<NameMapping>());
            }

            var nameMappings = await _namesRepository.GetAllNameMappings(pluginNamesRequest.Name);
            return Ok(nameMappings);
        }
    }
}
using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace AppStoreIntegrationServiceManagement.Controllers.Settings
{
    [Authorize(Policy = "IsAdmin")]
    [Route("Settings/[controller]")]
    public class ExportPluginsController : Controller
    {
        private readonly IPluginRepository _pluginRepository;

        public ExportPluginsController(IPluginRepository pluginRepository)
        {
            _pluginRepository = pluginRepository;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return View("Views/Settings/ExportPlugins.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> Export()
        {
            var response = new PluginsResponse { Value = await _pluginRepository.GetAll("asc") };
            var jsonString = JsonConvert.SerializeObject(response);
            var stream = Encoding.UTF8.GetBytes(jsonString);
            return File(stream, "application/octet-stream", "ExportPluginsConfig.json");
        }
    }
}

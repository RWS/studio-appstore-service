using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceManagement.Model.DataBase;
using AppStoreIntegrationServiceManagement.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AppStoreIntegrationServiceManagement.Controllers.Settings
{
    [Area("Settings")]
    [Authorize(Roles = "Administrator, Developer")]
    public class PluginsRenameController : Controller
    {
        private readonly INamesRepository _namesRepository;
        private readonly IPluginRepository _pluginRepository;

        public PluginsRenameController(INamesRepository namesRepository, IPluginRepository pluginRepository)
        {
            _namesRepository = namesRepository;
            _pluginRepository = pluginRepository;
        }

        [Route("Settings/PluginsRename")]
        public async Task<IActionResult> Index()
        {
            var username = User.Identity.Name;
            var userRole = IdentityUserExtended.GetUserRole((ClaimsIdentity)User.Identity);
            var plugins = await _pluginRepository.GetAll("asc", username, userRole);
            var names = await _namesRepository.GetAllNames();
            return View(plugins.SelectMany(p => names.Where(n => n.OldName.Equals(p.Name))));
        }

        [HttpPost]
        public async Task<IActionResult> Add() 
        {
            var mappings = await _namesRepository.GetAllNames();
            return PartialView("_NewNameMappingPartial", new NameMapping
            {
                Id = SetIndex(mappings),
                OldName = "",
                NewName = ""
            });
        }

        [HttpPost]
        public async Task<IActionResult> Update(NameMapping mapping)
        {
            if (await _namesRepository.TryUpdateMapping(mapping))
            {
                await _namesRepository.TryUpdateMapping(mapping);
                TempData["StatusMessage"] = "Success! Name mapping was updated!";
                return Content("/Settings/PluginsRename");
            }
                
            return PartialView("_StatusMessage", "Error! Parameter cannot be null!");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            await _namesRepository.DeleteMapping(id);
            TempData["StatusMessage"] = "Success! Name mapping was deleted!";
            return Content("/Settings/PluginsRename");
        }

        private static string SetIndex(IEnumerable<NameMapping> mappings)
        {
            var lastNameMapping = mappings.LastOrDefault();
            if (lastNameMapping == null)
            {
                return "1";
            }

            return (int.Parse(lastNameMapping.Id) + 1).ToString();
        }
    }
}

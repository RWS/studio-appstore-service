using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppStoreIntegrationServiceManagement.Controllers.Settings
{
    [Area("Settings")]
    public class PluginsRenameController : Controller
    {
        private readonly INamesRepository _namesRepository;

        public PluginsRenameController(INamesRepository namesRepository)
        {
            _namesRepository = namesRepository;
        }

        [Route("Settings/PluginsRename")]
        public async Task<IActionResult> Index()
        {
            return View(await _namesRepository.GetAllNameMappings());
        }

        [HttpPost]
        public async Task<IActionResult> Add() 
        {
            var mappings = await _namesRepository.GetAllNameMappings();
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

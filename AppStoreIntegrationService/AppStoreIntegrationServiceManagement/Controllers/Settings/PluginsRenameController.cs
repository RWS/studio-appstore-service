using AppStoreIntegrationServiceCore.DataBase.Interface;
using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceManagement.DataBase.Interface;
using AppStoreIntegrationServiceManagement.Filters;
using AppStoreIntegrationServiceManagement.Model;
using AppStoreIntegrationServiceManagement.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppStoreIntegrationServiceManagement.Controllers.Settings
{
    [Area("Settings")]
    [Authorize]
    [DBSynched]
    [AccountSelect]
    [TechPartnerAgreement]
    [RoleAuthorize("System Administrator", "Administrator")]
    public class PluginsRenameController : CustomController
    {
        private readonly INamesRepository _namesRepository;
        private readonly IPluginRepository _pluginRepository;

        public PluginsRenameController
        (
            IUserProfilesManager userProfilesManager,
            IUserAccountsManager userAccountsManager,
            INamesRepository namesRepository, 
            IPluginRepository pluginRepository,
            IAccountsManager accountsManager
        ) : base(userProfilesManager, userAccountsManager, accountsManager)
        {
            _namesRepository = namesRepository;
            _pluginRepository = pluginRepository;
        }

        [Route("Settings/PluginsRename")]
        public async Task<IActionResult> Index()
        {
            var plugins = await _pluginRepository.GetAll("asc", User.Identity.Name, ExtendedUser.Role);
            var names = await _namesRepository.GetAllNames();
            return View(plugins.SelectMany(p => names.Where(n => n.OldName.Equals(p.Name))));
        }

        [HttpPost]
        public async Task<IActionResult> Add() 
        {
            return PartialView("_NewNameMappingPartial", new NameMapping
            {
                Id = SetIndex(await _namesRepository.GetAllNames())
            });
        }

        [HttpPost]
        public async Task<IActionResult> Update(NameMapping mapping)
        {
            if (await _namesRepository.TryUpdateMapping(mapping))
            {
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

using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using AppStoreIntegrationServiceManagement.Model.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AppStoreIntegrationServiceManagement.Controllers.Settings
{
    [Authorize(Policy = "IsAdmin")]
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
            return View(new PluginsRenameModel
            {
                NamesMapping = await _namesRepository.GetAllNameMappings()
            });
        }

        [HttpPost]
        public IActionResult AddNew(IEnumerable<NameMapping> mappings) 
        {
            return PartialView("_NewNameMappingPartial", new NameMapping
            {
                Id = SetIndex(mappings),
                OldName = "",
                NewName = ""
            });
        }

        [HttpPost]
        public async Task<IActionResult> Add(NameMapping mapping, List<NameMapping> mappings)
        {
            if (IsValidNameMapping(mapping))
            {
                mappings.Add(mapping);
                await _namesRepository.UpdateNamesMapping(mappings);
                return RedirectToAction("Index");
            }

            var modalDetails = new ModalMessage
            {
                Title = string.Empty,
                Message = "Parameter cannot be null!",
                ModalType = ModalType.WarningMessage
            };

            return PartialView("_ModalPartial", modalDetails);
        }

        [HttpPost]
        public async Task<IActionResult> Update(List<NameMapping> mappings)
        {
            if (!mappings.Any(item => string.IsNullOrEmpty(item.OldName) || string.IsNullOrEmpty(item.NewName)))
            {
                await _namesRepository.UpdateNamesMapping(mappings);
                return RedirectToAction("Index");
            }

            var modalDetails = new ModalMessage
            {
                Title = string.Empty,
                Message = "Parameter cannot be null!",
                ModalType = ModalType.WarningMessage
            };

            return PartialView("_ModalPartial", modalDetails);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            await _namesRepository.DeleteNameMapping(id);
            return RedirectToAction("Index");
        }

        [Route("[controller]/[action]/{pageUrl?}")]
        [HttpPost]
        public async Task<IActionResult> GoToPage(string pageUrl, NameMapping mapping, List<NameMapping> mappings)
        {
            if (string.IsNullOrEmpty(mapping.NewName) &&
                string.IsNullOrEmpty(mapping.OldName) &&
                await HaveUnsavedChanges(mappings))
            {
                return Redirect(pageUrl.Replace('.', '/'));
            }

            var modalDetails = new ModalMessage
            {
                ModalType = ModalType.WarningMessage,
                Title = "Warning!",
                Message = $"There are unsaved changes for plugins rename. Discard changes?",
                RequestPage = $"{pageUrl.Replace('.', '/')}"
            };

            return PartialView("_ModalPartial", modalDetails);
        }

        private async Task<bool> HaveUnsavedChanges(List<NameMapping> mappings)
        {
            var savedNamesMapping = (await _namesRepository.GetAllNameMappings()).ToList();
            return JsonConvert.SerializeObject(savedNamesMapping) == JsonConvert.SerializeObject(mappings);
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

        private static bool IsValidNameMapping(NameMapping mapping)
        {
            return !string.IsNullOrEmpty(mapping.NewName) &&
                   !string.IsNullOrEmpty(mapping.OldName);
        }
    }
}

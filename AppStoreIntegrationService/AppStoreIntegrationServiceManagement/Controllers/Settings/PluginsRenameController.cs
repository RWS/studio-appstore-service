using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using AppStoreIntegrationServiceManagement.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AppStoreIntegrationServiceManagement.Controllers.Settings
{
    [Authorize(Policy = "IsAdmin")]
    [Route("Settings/[controller]")]
    public class PluginsRenameController : Controller
    {
        private readonly INamesRepository _namesRepository;

        public PluginsRenameController(INamesRepository namesRepository)
        {
            _namesRepository = namesRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return View("Views/Settings/PluginsRename.cshtml", new PluginsRenameModel
            {
                NamesMapping = await _namesRepository.GetAllNameMappings()
            });
        }

        [Route("/Settings/PluginsRename/AddNewNameMapping")]
        [HttpPost]
        public IActionResult AddNewNameMapping(IEnumerable<NameMapping> mappings) 
        {
            return PartialView("/Views/Settings/_NewNameMappingPartial.cshtml", new NameMapping
            {
                Id = SetIndex(mappings),
                OldName = "",
                NewName = ""
            });
        }

        [Route("/Settings/PluginsRename/AddNameMapping")]
        [HttpPost]
        public async Task<IActionResult> AddNameMapping(NameMapping mapping, List<NameMapping> mappings)
        {
            if (IsValidNameMapping(mapping))
            {
                mappings.Add(mapping);
                await _namesRepository.UpdateNamesMapping(mappings);
                return RedirectToAction("Get");
            }

            var modalDetails = new ModalMessage
            {
                Title = string.Empty,
                Message = "Parameter cannot be null!",
                ModalType = ModalType.WarningMessage
            };

            return PartialView("/Views/_ModalPartial.cshtml", modalDetails);
        }

        [Route("/Settings/PluginsRename/UpdateNamesMapping")]
        [HttpPost]
        public async Task<IActionResult> UpdateNamesMapping(List<NameMapping> mappings)
        {
            if (!mappings.Any(item => string.IsNullOrEmpty(item.OldName) || string.IsNullOrEmpty(item.NewName)))
            {
                await _namesRepository.UpdateNamesMapping(mappings);
                return RedirectToAction("Get");
            }

            var modalDetails = new ModalMessage
            {
                Title = string.Empty,
                Message = "Parameter cannot be null!",
                ModalType = ModalType.WarningMessage
            };

            return PartialView("/Views/_ModalPartial.cshtml", modalDetails);
        }

        [Route("/Settings/PluginsRename/DeleteNameMapping/{id}")]
        [HttpPost]
        public async Task<IActionResult> DeleteNameMapping(string id)
        {
            await _namesRepository.DeleteNameMapping(id);
            return RedirectToAction("Get");
        }

        [Route("/Settings/PluginsRename/GoToPage/{pageUrl}")]
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

            return PartialView("/Views/_ModalPartial.cshtml", modalDetails);
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

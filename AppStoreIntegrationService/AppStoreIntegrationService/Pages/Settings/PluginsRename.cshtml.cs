using AppStoreIntegrationService.Model;
using AppStoreIntegrationService.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppStoreIntegrationService.Pages.Settings
{
    public class PluginsRenameModel : PageModel
    {
        private readonly IConfigurationSettings _configurationSettings;
        private readonly INamesRepository _namesRepository;

        public PluginsRenameModel(IConfigurationSettings configurationSettings, INamesRepository namesRepository)
        {
            _configurationSettings = configurationSettings;
            _namesRepository = namesRepository;
        }

        [BindProperty]
        public List<NameMapping> NamesMapping { get; set; }

        [BindProperty]
        public NameMapping NewNameMapping { get; set; }

        public async Task<IActionResult> OnGet()
        {
            NamesMapping = (await _namesRepository.GetAllNameMappings()).ToList();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteNameMapping(string id)
        {
            await _namesRepository.DeleteNameMapping(id);
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateNamesMapping()
        {
            if (!NamesMapping.Any(item => string.IsNullOrEmpty(item.OldName) || string.IsNullOrEmpty(item.NewName)))
            {
                await _namesRepository.UpdateNamesMapping(NamesMapping);
                return Page();
            }

            var modalDetails = new ModalMessage
            {
                Title = string.Empty,
                Message = "Parameter cannot be null!",
                ModalType = ModalType.WarningMessage
            };

            return Partial("_ModalPartial", modalDetails);
        }

        public async Task<IActionResult> OnPostAddNewNameMapping()
        {
            NewNameMapping = new NameMapping
            {
                Id = SetIndex(),
                OldName = "",
                NewName = ""
            };

            return Partial("_NewNameMappingPartial", NewNameMapping);
        }

        private string SetIndex()
        {
            var lastNameMapping = NamesMapping.LastOrDefault();
            if (lastNameMapping == null)
            {
                return "1";
            }

            return (int.Parse(lastNameMapping.Id) + 1).ToString();
        }

        public async Task<IActionResult> OnPostAddNameMapping()
        {
            if (IsValidNameMapping())
            {
                NamesMapping.Add(NewNameMapping);
                await _namesRepository.UpdateNamesMapping(NamesMapping);
                return Page();
            }

            var modalDetails = new ModalMessage
            {
                Title = string.Empty,
                Message = "Parameter cannot be null!",
                ModalType = ModalType.WarningMessage
            };

            return Partial("_ModalPartial", modalDetails);
        }

        private bool IsValidNameMapping()
        {
            return !string.IsNullOrEmpty(NewNameMapping.NewName) &&
                   !string.IsNullOrEmpty(NewNameMapping.OldName);
        }
    }
}

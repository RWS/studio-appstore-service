using AppStoreIntegrationService.Model;
using AppStoreIntegrationService.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppStoreIntegrationService.Pages
{
    //[Authorize(Roles = "Administrator")]
    public class Settings : PageModel
    {
        private readonly IPluginRepository _repository;
        private readonly IWritableOptions<SiteSettings> _options;
        private readonly IConfigurationSettings _configurationSettings;
        private readonly INamesRepository _namesRepository;

        public Settings(IPluginRepository repository, IWritableOptions<SiteSettings> options, IConfigurationSettings configurationSettings, INamesRepository namesRepository)
        {
            _configurationSettings = configurationSettings;
            _namesRepository = namesRepository;
            _repository = repository;
            _options = options;
        }

        [BindProperty]
        public List<NameMapping> NamesMapping { get; set; }

        [BindProperty]
        public NameMapping NewNameMapping { get; set; }

        [BindProperty]
        public string SiteName { get; set; }

        [BindProperty]
        public IFormFile ImportedFile { get; set; }

        public async Task<IActionResult> OnGet()
        {
            SiteName = _options.Value.Name;
            NamesMapping = await _namesRepository.ReadLocalNameMappings(_configurationSettings.NameMappingsFilePath);
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteNameMapping(string id)
        {
            await _namesRepository.DeleteNameMappingById(_configurationSettings.NameMappingsFilePath, id);
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateNamesMapping()
        {
            if (!NamesMapping.Any(item => string.IsNullOrEmpty(item.OldName) || string.IsNullOrEmpty(item.NewName)))
            {
                await _namesRepository.UpdateLocalNamesMapping(_configurationSettings.NameMappingsFilePath, NamesMapping);
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
                await _namesRepository.UpdateLocalNamesMapping(_configurationSettings.NameMappingsFilePath, NamesMapping);
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

        public async Task<IActionResult> OnPostExportPlugins()
        {
            var response = new PluginsResponse { Value = await _repository.GetAll("asc") };
            var jsonString = JsonConvert.SerializeObject(response);
            var stream = Encoding.UTF8.GetBytes(jsonString);
            return File(stream, "application/octet-stream", "ExportPluginsConfig.json");
        }

        public async Task<IActionResult> OnPostImportFile()
        {
            var modalDetails = new ModalMessage();
            var success = await _repository.TryImportPluginsFromFile(ImportedFile);
            if (success)
            {
                modalDetails.RequestPage = "/ConfigTool";
                modalDetails.ModalType = ModalType.SuccessMessage;
                modalDetails.Title = "Success!";
                modalDetails.Message = $"The file content was imported! Return to plugins list?";
            }
            else
            {
                modalDetails.Title = string.Empty;
                modalDetails.Message = "The file is empty or in wrong format!";
                modalDetails.ModalType = ModalType.WarningMessage;
            }

            return Partial("_ModalPartial", modalDetails);
        }

        public async Task<IActionResult> OnPostSaveSiteName()
        {
            _options.SaveOption(new SiteSettings { Name = SiteName });
            _options.Value.Name = SiteName;
            return Redirect("Settings");
        }

        private bool IsValidNameMapping()
        {
            return !string.IsNullOrEmpty(NewNameMapping.NewName) &&
                   !string.IsNullOrEmpty(NewNameMapping.OldName);
        }
    }
}

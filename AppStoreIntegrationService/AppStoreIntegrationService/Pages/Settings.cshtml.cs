using AppStoreIntegrationService.Model;
using AppStoreIntegrationService.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;

namespace AppStoreIntegrationService.Pages
{
    [Authorize(Roles = "Administrator")]
    public class Settings : PageModel
    {
        private readonly IPluginRepository _repository;
        private readonly IWritableOptions<SiteSettings> _options;

        public Settings(IPluginRepository repository, IWritableOptions<SiteSettings> options)
        {
            _repository = repository;
            _options = options;
        }

        [BindProperty]
        public string SiteName { get; set; }

        [BindProperty]
        public IFormFile ImportedFile { get; set; }

        public IActionResult OnGet()
        {
            SiteName = _options.Value.Name;
            return Page();
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
                modalDetails.RequestPage = "config";
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
    }
}

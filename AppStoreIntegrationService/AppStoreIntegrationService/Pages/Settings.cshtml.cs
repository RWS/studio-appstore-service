using AppStoreIntegrationService.Model;
using AppStoreIntegrationService.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AppStoreIntegrationService.Pages
{
    [Authorize(Roles = "Administrator")]
    public class Settings : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IPluginRepository _repository;

        public Settings(IConfiguration configuration, IPluginRepository repository)
        {
            _configuration = configuration;
            _repository = repository;
        }

        [BindProperty]
        public string SiteName { get; set; }

        [BindProperty]
        public IFormFile ImportedFile { get; set; }

        public async Task<IActionResult> OnGet()
        {
            return Page();
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
        
        public async Task<FileResult> OnGetExportPlugins()
        {
            var response = new PluginsResponse { Value = await _repository.GetAll("asc") };
            var jsonString = JsonConvert.SerializeObject(response);
            var stream = Encoding.UTF8.GetBytes(jsonString);
            return File(stream, "application/octet-stream", "ExportPluginsConfig.json");
        }

        public async Task<IActionResult> OnPostSaveSiteName()
        {
            var settingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            var settingsAsText = await System.IO.File.ReadAllTextAsync(settingsPath);
            var settingsAsObject = JsonConvert.DeserializeObject<dynamic>(settingsAsText);
            settingsAsObject.SiteName = SiteName;
            await System.IO.File.WriteAllTextAsync(settingsPath, JsonConvert.SerializeObject(settingsAsObject));
            _configuration["SiteName"] = SiteName;

            return Redirect("Settings");
        }
    }
}

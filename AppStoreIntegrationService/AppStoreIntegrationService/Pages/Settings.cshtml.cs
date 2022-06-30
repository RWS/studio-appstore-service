using AppStoreIntegrationService.Model;
using AppStoreIntegrationService.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
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
    [Authorize]
    public class Settings : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IPluginRepository _repository;
        private readonly IWebHostEnvironment _env;

        public Settings(IConfiguration configuration, IPluginRepository repository, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _repository = repository;
            _env = env;
        }

        [BindProperty]
        public string SiteName { get; set; }

        [BindProperty]
        public IFormFile SelectedFile { get; set; }

        public async Task<IActionResult> OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostImportFile()
        {
            var modalDetails = new ModalMessage();
            var response = await _repository.ImportFromFile(SelectedFile);
            var statusCode = (response as StatusCodeResult).StatusCode;
            if (statusCode.Equals(200))
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
            var pluginsAsText = JsonConvert.SerializeObject(new PluginsResponse { Value = await _repository.GetAll("asc") });
            return File(Encoding.UTF8.GetBytes(pluginsAsText), "application/octet-stream", "ExportPluginsConfig.json");
        }

        public async Task<IActionResult> OnPostSaveSiteName()
        {
            var appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            var oldName = _configuration["SiteName"];
            var newContent = System.IO.File.ReadAllText(appSettingsPath).Replace($"\"SiteName\": \"{oldName}\"", $"\"SiteName\": \"{SiteName}\"");
            await System.IO.File.WriteAllTextAsync(appSettingsPath, newContent);
            _configuration["SiteName"] = SiteName;
            return Redirect("Settings");
        }
    }
}

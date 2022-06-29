using AppStoreIntegrationService.Model;
using AppStoreIntegrationService.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AppStoreIntegrationService.Pages
{
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
            var response = await _repository.ImportFromFile(ImportedFile);
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
    }
}

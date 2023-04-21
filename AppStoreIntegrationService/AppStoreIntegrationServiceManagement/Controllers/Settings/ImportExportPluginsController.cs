using AppStoreIntegrationServiceCore.DataBase.Interface;
using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using AppStoreIntegrationServiceManagement.DataBase.Interface;
using AppStoreIntegrationServiceManagement.Filters;
using AppStoreIntegrationServiceManagement.Model;
using AppStoreIntegrationServiceManagement.Model.Settings;
using AppStoreIntegrationServiceManagement.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace AppStoreIntegrationServiceManagement.Controllers.Settings
{
    [Area("Settings")]
    [Authorize]
    [DBSynched]
    [AccountSelect]
    [TechPartnerAgreement]
    public class ImportExportPluginsController : CustomController
    {
        private readonly IPluginRepository _pluginRepository;
        private readonly IResponseManager _responseManager;

        public ImportExportPluginsController
        (
            IUserProfilesManager userProfilesManager,
            IUserAccountsManager userAccountsManager,
            IPluginRepository pluginRepository, 
            IResponseManager responseManager,
            IAccountsManager accountsManager
        ) : base(userProfilesManager, userAccountsManager, accountsManager)
        {
            _pluginRepository = pluginRepository;
            _responseManager = responseManager;
        }

        [Route("Settings/ExportPlugins")]
        [RoleAuthorize("System Administrator", "Administrator")]
        public IActionResult Export()
        {
            return View();
        }

        [HttpPost]
        [RoleAuthorize("System Administrator", "Administrator")]
        public async Task<IActionResult> CreateExport()
        {
            var response = await _responseManager.GetResponse();
            response.Value = await _pluginRepository.GetAll("asc", User.Identity.Name, ExtendedUser.Role);
            var stream = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response));
            return File(stream, "application/octet-stream", "ExportPluginsConfig.json");
        }

        [Route("Settings/ImportPlugins")]
        [RoleAuthorize("System Administrator")]
        public IActionResult Import()
        {
            return View(new ImportPluginsModel());
        }

        [HttpPost]
        [RoleAuthorize("System Administrator")]
        public async Task<IActionResult> CreateImport(ImportPluginsModel import)
        {
            var modalDetails = new ModalMessage
            {
                Title = "Warning!",
                Message = "The file is empty or in wrong format!",
                ModalType = ModalType.WarningMessage
            };

            var success = TryImportFromFile(import.ImportedFile, out var response);

            if (success)
            {
                await _responseManager.SaveResponse(response);

                modalDetails.RequestPage = "/Plugins";
                modalDetails.ModalType = ModalType.SuccessMessage;
                modalDetails.Title = "Success!";
                modalDetails.Message = $"The file content was imported! Return to plugins list?";
            }

            return PartialView("_ModalPartial", modalDetails);
        }

        private static bool TryImportFromFile(IFormFile file, out PluginResponse<PluginDetails> response)
        {
            var result = new StringBuilder();
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                while (reader.Peek() >= 0)
                {
                    result.AppendLine(reader.ReadLine());
                }
            }

            if (!string.IsNullOrEmpty(result.ToString()))
            {
                try
                {
                    response = JsonConvert.DeserializeObject<PluginResponse<PluginDetails>>(result.ToString());
                    return true;

                }
                catch (JsonException)
                {
                    response = null;
                    return false;
                }
            }

            response = null;
            return false;
        }
    }
}

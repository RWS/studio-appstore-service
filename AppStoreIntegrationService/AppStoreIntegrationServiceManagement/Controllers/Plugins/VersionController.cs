using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using AppStoreIntegrationServiceManagement.Model.Plugins;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NuGet.Protocol.Plugins;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceManagement.Controllers.Plugins
{
    [Area("Plugins")]
    [Authorize]
    public class VersionController : Controller
    {
        private readonly IPluginRepository _pluginRepository;
        private readonly IProductsRepository _productsRepository;
        private readonly ICommentsRepository _commentsRepository;
        private readonly ILoggingRepository _loggingRepository;

        public VersionController
        (
            IPluginRepository pluginRepository,
            IProductsRepository productsRepository,
            ICommentsRepository commentsRepository,
            ILoggingRepository loggingRepository
        )
        {
            _pluginRepository = pluginRepository;
            _productsRepository = productsRepository;
            _commentsRepository = commentsRepository;
            _loggingRepository = loggingRepository;
        }

        [HttpGet("/Plugins/Edit/{id}/Versions")]
        public async Task<IActionResult> Index(int id)
        {
            var selectedVersion = Request.Query["selectedVersion"];
            var plugin = new ExtendedPluginDetails(await _pluginRepository.GetPluginById(id, User))
            {
                Parents = await _productsRepository.GetAllParents(),
                IsEditMode = true
            };

            var products = new MultiSelectList(await _productsRepository.GetAllProducts(), nameof(ProductDetails.Id), nameof(ProductDetails.ProductName));
            var versions = (await Task.WhenAll(plugin.Versions?.Select(async v => new ExtendedPluginVersion(v)
            {
                VersionComments = await _commentsRepository.GetComments(plugin.Name, v.VersionId),
                SupportedProductsListItems = products,
                PluginId = id,
                IsThirdParty = plugin.IsThirdParty
            }))).ToList();

            if (plugin.IsThirdParty && !User.IsInRole("Developer"))
            {
                return View((plugin, versions));
            }

            versions.Add(new ExtendedPluginVersion
            {
                VersionId = versions.Any(v => v.VersionId == selectedVersion) || string.IsNullOrEmpty(selectedVersion) ? Guid.NewGuid().ToString() : selectedVersion,
                SupportedProductsListItems = products,
                IsNewVersion = true,
                PluginId = id,
                IsThirdParty = plugin.IsThirdParty
            });

            return View((plugin, versions));
        }

        [HttpPost("/Plugins/Edit/{pluginId}/Versions/Save")]
        public async Task<IActionResult> Save(PluginVersion version, bool isNewVersion, int pluginId)
        {
            try
            {
                version.HasAdminConsent = version.HasAdminConsent || version.VersionStatus.Equals(Status.InReview);
                var old = await _pluginRepository.GetPluginVersion(pluginId, version.VersionId, User);
                await _loggingRepository.Log(User, pluginId, new PluginVersionBase<string>(version), old == null ? old : new PluginVersionBase<string>(old));
                await _pluginRepository.UpdatePluginVersion(pluginId, version);
                var response = await PluginPackage.DownloadPlugin(version.DownloadUrl);
                var log = response.CreateVersionMatchLog(version, await _productsRepository.GetAllProducts(), out bool isFullMatch);

                TempData["IsVersionMatch"] = log.IsVersionMatch;
                TempData["IsMinVersionMatch"] = log.IsMinVersionMatch;
                TempData["IsMaxVersionMatch"] = log.IsMaxVersionMatch;
                TempData["IsProductMatch"] = log.IsProductMatch;

                if (!isFullMatch)
                {
                    TempData["StatusMessage"] = "Warning! Version was saved but there are manifest conflicts!";
                    return new EmptyResult();
                }
            }
            catch (Exception e)
            {
                TempData["StatusMessage"] = $"Error! {e.Message}.";
                return new EmptyResult();
            }

            TempData["StatusMessage"] = string.Format("Success! Version was {0}!", isNewVersion ? "added" : "updated");
            return new EmptyResult();
        }

        [HttpPost]
        public async Task<IActionResult> GenerateChecksum(string downloadUrl)
        {
            try
            {
                var remoteReader = new RemoteStreamReader(new Uri(downloadUrl));
                var filehash = SHA1Generator.GetHash(await remoteReader.ReadAsStreamAsync());
                TempData["Filehash"] = filehash;
                return PartialView("_StatusMessage", "Success! Checksum was generated!");
            }
            catch (Exception e)
            {
                return PartialView("_StatusMessage", $"Error! {e.Message}");
            }
        }

        [HttpPost("/Plugins/Edit/{pluginId}/Versions/Delete/{versionId}")]
        public async Task<IActionResult> Delete(int pluginId, string versionId, bool deletionApproval)
        {
            var version = await _pluginRepository.GetPluginVersion(pluginId, versionId, User);

            if (User.IsInRole("Developer") && version.HasAdminConsent ||
               (User.IsInRole("Administrator") && !deletionApproval))
            {
                version.NeedsDeletionApproval = deletionApproval;
                await _pluginRepository.UpdatePluginVersion(pluginId, version);
                TempData["StatusMessage"] = string.Format("Success! Version deletion request was {0}!", deletionApproval ? "sent" : "rejected");
                return Content(null);
            }

            await _loggingRepository.Log(User, pluginId, $"<b>{User.Identity.Name}</b> removed the version with number <b>{version.VersionNumber}</b> at {DateTime.Now}");
            await _pluginRepository.RemovePluginVersion(pluginId, versionId);
            TempData["StatusMessage"] = "Success! Version was removed!";
            return Content(null);
        }
    }
}

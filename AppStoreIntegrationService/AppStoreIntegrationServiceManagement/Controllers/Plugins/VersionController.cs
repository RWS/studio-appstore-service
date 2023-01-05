using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using AppStoreIntegrationServiceManagement.Model.Plugins;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppStoreIntegrationServiceManagement.Controllers.Plugins
{
    [Area("Plugins")]
    [Authorize]
    public class VersionController : Controller
    {
        private readonly IPluginRepository _pluginRepository;
        private readonly IProductsRepository _productsRepository;
        private readonly ICommentsRepository _commentsRepository;

        public VersionController(IPluginRepository pluginRepository, IProductsRepository productsRepository, ICommentsRepository commentsRepository)
        {
            _pluginRepository = pluginRepository;
            _productsRepository = productsRepository;
            _commentsRepository = commentsRepository;
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
                VersionComments = await _commentsRepository.GetCommentsForVersion(plugin.Name, v.VersionId),
                SupportedProductsListItems = products,
                PluginId = id
            }))).ToList();

            versions.Add(new ExtendedPluginVersion
            {
                VersionId = versions.Any(v => v.VersionId == selectedVersion) || string.IsNullOrEmpty(selectedVersion) ? Guid.NewGuid().ToString() : selectedVersion,
                SupportedProductsListItems = products,
                IsNewVersion = true,
                PluginId = id
            });
            return View((plugin, versions));
        }

        [HttpPost("/Plugins/Edit/{pluginId}/Versions/Save")]
        public async Task<IActionResult> Save(ExtendedPluginVersion version, int pluginId)
        {
            try
            {
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
            }

            TempData["StatusMessage"] = string.Format("Success! Version was {0}!", version.IsNewVersion ? "added" : "updated");
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
        public async Task<IActionResult> Delete(int pluginId, string versionId)
        {
            await _pluginRepository.RemovePluginVersion(pluginId, versionId);
            TempData["StatusMessage"] = "Success! Version was removed!";
            return Content(null);
        }
    }
}

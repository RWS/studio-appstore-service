using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using AppStoreIntegrationServiceManagement.Model;
using Microsoft.AspNetCore.Mvc;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceManagement.Controllers.Preservation
{
    public class PreservationController : Controller
    {
        private readonly IPluginRepository _pluginRepository;
        private readonly ICommentsRepository _commentsRepository;

        public PreservationController(IPluginRepository pluginRepository, ICommentsRepository commentsRepository)
        {
            _pluginRepository = pluginRepository;
            _commentsRepository = commentsRepository;
        }

        [HttpPost("/Preservation/Check")]
        public async Task<IActionResult> Check
        (
            ExtendedPluginVersion version,
            ExtendedPluginDetails plugin,
            Comment comment,
            Status status,
            Page page
        )
        {
           return page switch
            {
                Page.Plugin => await Check(plugin, status),
                Page.Version => await Check(version),
                Page.Comment => await Check(comment),
                _ => Content(null)
            };
        }

        public async Task<IActionResult> Check(ExtendedPluginVersion version)
        {
            var plugin = await _pluginRepository.GetPluginById(version.PluginId, User.IsInRole("Developer") ? User.Identity.Name : null);
            var saved = plugin?.Versions.FirstOrDefault(v => v.VersionId == version.VersionId);

            if (saved?.Equals(new PluginVersion<string>(version)) ?? true)
            {
                return Content(null);
            }

            return PartialView("_ModalPartial", new ModalMessage
            {
                Message = "Discard changes for this version?"
            });
        }

        public async Task<IActionResult> Check(ExtendedPluginDetails plugin, Status status)
        {
            var saved = await _pluginRepository.GetPluginById(plugin.Id, User.IsInRole("Developer") ? User.Identity.Name : null);

            if (saved?.Equals(new PluginDetails<PluginVersion<string>, string>(plugin) { Status = status }) ?? true)
            {
                return Content(null);
            }

            return PartialView("_ModalPartial", new ModalMessage
            {
                Message = "Discard changes for this plugin?"
            });
        }

        public async Task<IActionResult> Check(Comment comment)
        {
            var plugin = await _pluginRepository.GetPluginById(comment.PluginId);
            var saved = await _commentsRepository.GetVersionComment(plugin.Name, comment.CommentId, comment.VersionId);

            if (saved?.Equals(comment) ?? true)
            {
                return Content(null);
            }

            return PartialView("_ModalPartial", new ModalMessage
            {
                Message = "Discard changes for this comment?"
            });
        }
    }
}

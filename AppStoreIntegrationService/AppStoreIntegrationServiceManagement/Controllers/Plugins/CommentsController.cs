using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AppStoreIntegrationServiceManagement.Repository.Interface;
using AppStoreIntegrationServiceManagement.Model.Plugins;
using AppStoreIntegrationServiceManagement.Model.Notifications;
using AppStoreIntegrationServiceManagement.Model;
using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceManagement.Controllers.Plugins
{
    [Area("Plugins")]
    [Authorize]
    public class CommentsController : CustomController
    {
        private readonly ICommentsRepository _commentsRepository;
        private readonly IPluginRepository _pluginRepository;
        private readonly IPluginVersionRepository _pluginVersionRepository;
        private readonly INotificationCenter _notificationCenter;

        public CommentsController
        (
            ICommentsRepository commentsRepository,
            IPluginRepository pluginRepository,
            IPluginVersionRepository pluginVersionRepository,
            INotificationCenter notificationCenter
        )
        {
            _commentsRepository = commentsRepository;
            _pluginRepository = pluginRepository;
            _pluginVersionRepository = pluginVersionRepository;
            _notificationCenter = notificationCenter;
        }

        [Route("/Plugins/Edit/{pluginId}/Comments")]
        public async Task<IActionResult> Index(int pluginId)
        {
            var plugin = await _pluginRepository.GetPluginById(pluginId);
            var comments = await _commentsRepository.GetComments(pluginId);
            var extended = ExtendedPluginDetails.CopyFrom(plugin);
            extended.Comments = comments;
            extended.IsEditMode = true;

            return View(extended);
        }

        [Route("/Plugins/Edit/{pluginId}/Versions/Edit/{versionId}/Comments")]
        public async Task<IActionResult> VersionComments(int pluginId, string versionId)
        {
            var plugin = await _pluginRepository.GetPluginById(pluginId);
            var version = await _pluginVersionRepository.GetPluginVersion(pluginId, versionId);
            var extendedPlugin = ExtendedPluginDetails.CopyFrom(plugin);
            var extendedVersion = ExtendedPluginVersion.CopyFrom(version);

            extendedVersion.VersionComments = await _commentsRepository.GetComments(pluginId, versionId);
            extendedVersion.PluginId = pluginId;
            extendedPlugin.IsEditMode = true;

            return View((extendedPlugin, extendedVersion));
        }

        public async Task<IActionResult> New(int pluginId, string versionId)
        {
            var comments = await _commentsRepository.GetComments(pluginId, versionId);
            return PartialView("_NewCommentPartial", new Comment
            {
                CommentId = comments.LastOrDefault()?.CommentId + 1 ?? 0,
                CommentAuthor = User.Identity.Name,
                CommentDate = DateTime.Now,
                PluginId = pluginId,
                VersionId = versionId
            });
        }

        public async Task<IActionResult> Update(Comment comment, int pluginId, string versionId)
        {
            var plugin = await _pluginRepository.GetPluginById(pluginId);

            var notification = new EmailNotification(plugin)
            {
                CallToActionUrl = $"{GetUrlBase()}/Plugins/Draft/{plugin.Id}",
                Message = $"There is a new commet for a plugin{(string.IsNullOrEmpty(versionId) ? null : " version")}!"
            };

            await Notify(notification, new PushNotification(notification));
            await _commentsRepository.SaveComment(comment, pluginId, versionId);
            TempData["StatusMessage"] = "Success! Comment was updated!";
            return Content(null);
        }

        private async Task Notify(EmailNotification emailNotification, PushNotification pushNotification)
        {
            await _notificationCenter.SendEmail(emailNotification);
            await _notificationCenter.Push(pushNotification);
            await _notificationCenter.Broadcast(emailNotification);
            pushNotification.Author = AccountsManager.GetAppStoreAccount().AccountName;
            await _notificationCenter.Push(pushNotification);
        }

        public async Task<IActionResult> Delete(int commentId, int pluginId, string versionId)
        {
            await _commentsRepository.DeleteComment(commentId, pluginId, versionId);
            TempData["StatusMessage"] = "Success! Comment was deleted!";
            return Content(null);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AppStoreIntegrationServiceManagement.Repository.Interface;
using AppStoreIntegrationServiceManagement.Repository;
using AppStoreIntegrationServiceManagement.Model.Comments;
using AppStoreIntegrationServiceManagement.Model.Plugins;
using Microsoft.AspNetCore.Identity;
using AppStoreIntegrationServiceManagement.Model.DataBase;

namespace AppStoreIntegrationServiceManagement.Controllers.Plugins
{
    [Area("Plugins")]
    [Authorize]
    public class CommentsController : Controller
    {
        private readonly ICommentsRepository _commentsRepository;
        private readonly IPluginRepository _pluginRepository;
        private readonly IPluginVersionRepository _pluginVersionRepository;
        private readonly INotificationCenter _notificationCenter;
        private readonly UserManager<IdentityUserExtended> _userManager;
        private readonly AccountsManager _accountsManager;

        public CommentsController
        (
            ICommentsRepository commentsRepository,
            IPluginRepository pluginRepository,
            IPluginVersionRepository pluginVersionRepository,
            INotificationCenter notificationCenter,
            UserManager<IdentityUserExtended> userManager,
            AccountsManager accountsManager
        )
        {
            _commentsRepository = commentsRepository;
            _pluginRepository = pluginRepository;
            _pluginVersionRepository = pluginVersionRepository;
            _notificationCenter = notificationCenter;
            _userManager = userManager;
            _accountsManager = accountsManager;
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
            var template = versionId == null ? NotificationTemplate.NewPluginComment : NotificationTemplate.NewVersionComment;
            var emailNotification = _notificationCenter.GetNotification(template, true, plugin.Icon.MediaUrl, plugin.Name, plugin.Id, versionId);
            var pushNotification = _notificationCenter.GetNotification(template, false, plugin.Icon.MediaUrl, plugin.Name, plugin.Id, versionId);
            var account = _accountsManager.GetAppStoreAccount();

            await _notificationCenter.SendEmail(emailNotification, plugin.Developer.DeveloperName);
            await _notificationCenter.Push(pushNotification, plugin.Developer.DeveloperName);
            await _notificationCenter.Broadcast(emailNotification);
            await _notificationCenter.Push(pushNotification, account.AccountName);
            await _commentsRepository.SaveComment(comment, pluginId, versionId);
            TempData["StatusMessage"] = "Success! Comment was updated!";
            return Content(null);
        }

        public async Task<IActionResult> Delete(int commentId, int pluginId, string versionId)
        {
            await _commentsRepository.DeleteComment(commentId, pluginId, versionId);
            TempData["StatusMessage"] = "Success! Comment was deleted!";
            return Content(null);
        }
    }
}

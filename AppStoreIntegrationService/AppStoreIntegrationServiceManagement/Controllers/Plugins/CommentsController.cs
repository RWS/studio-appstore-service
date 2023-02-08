﻿using AppStoreIntegrationServiceCore.Repository.Interface;
using AppStoreIntegrationServiceCore.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using static AppStoreIntegrationServiceCore.Enums;
using AppStoreIntegrationServiceManagement.Model.Plugins;
using Microsoft.AspNetCore.Identity;
using AppStoreIntegrationServiceManagement.Model.Identity;
using System.Security.Claims;

namespace AppStoreIntegrationServiceManagement.Controllers.Plugins
{
    [Area("Plugins")]
    [Authorize]
    public class CommentsController : Controller
    {
        private readonly ICommentsRepository _commentsRepository;
        private readonly IPluginRepository _pluginRepository;
        private readonly IPluginVersionRepository _pluginVersionRepository;
        private readonly NotificationCenter _notificationCenter;
        private readonly UserManager<IdentityUserExtended> _userManager;

        public CommentsController
        (
            ICommentsRepository commentsRepository,
            IPluginRepository pluginRepository,
            IPluginVersionRepository pluginVersionRepository,
            NotificationCenter notificationCenter,
            UserManager<IdentityUserExtended> userManager
        )
        {
            _commentsRepository = commentsRepository;
            _pluginRepository = pluginRepository;
            _pluginVersionRepository = pluginVersionRepository;
            _notificationCenter = notificationCenter;
            _userManager = userManager;
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
            var (emailNotification, pushNotification) = _notificationCenter.GetNewCommentNotification(plugin.Icon.MediaUrl, plugin.Name, plugin.Id, versionId);

            switch (User.IsInRole("Administrator"))
            {
                case true:
                    await _notificationCenter.SendEmail(emailNotification, "New plugin comment", await GetCurrentPluginUserEmail(plugin.Developer.DeveloperName));
                    await _notificationCenter.Push(pushNotification, User.Identity.Name);
                    break;
                default:
                    await _notificationCenter.Broadcast(emailNotification, "New plugin comment");
                    await _notificationCenter.Push(pushNotification);
                    break;
            }

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

        private async Task<string> GetCurrentPluginUserEmail(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            return user.Email;
        }
    }
}

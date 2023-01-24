using AppStoreIntegrationServiceCore.Repository.Interface;
using AppStoreIntegrationServiceCore.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceManagement.Controllers.Plugins
{
    [Area("Plugins")]
    [Authorize]
    public class CommentsController : Controller
    {
        private readonly ICommentsRepository _commentsRepository;
        private readonly IPluginRepository _pluginRepository;

        public CommentsController(ICommentsRepository commentsRepository, IPluginRepository pluginRepository)
        {
            _commentsRepository = commentsRepository;
            _pluginRepository = pluginRepository;
        }

        [Route("/Plugins/Edit/{pluginId}/Comments")]
        public async Task<IActionResult> Index(int pluginId)
        {
            var plugin = await _pluginRepository.GetPluginById(pluginId, Status.All, User);
            var comments = await _commentsRepository.GetComments(pluginId);
            var extended = ExtendedPluginDetails.CopyFrom(plugin);
            extended.Comments = comments;
            extended.IsEditMode = true;

            return View(extended);
        }

        [HttpPost("/Plugins/Edit/{pluginId}/Comments/{versionId}/New")]
        [HttpPost("/Plugins/Edit/{pluginId}/Comments/New")]
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

        [HttpPost("/Plugins/Edit/{pluginId}/Comments/{versionId}/Update")]
        [HttpPost("/Plugins/Edit/{pluginId}/Comments/Update")]
        public async Task<IActionResult> Update(Comment comment, int pluginId, string versionId)
        {
            await _commentsRepository.SaveComment(comment, pluginId, versionId);
            TempData["StatusMessage"] = "Success! Comment was updated!";
            return Content(null);
        }

        [HttpPost("/Plugins/Edit/{pluginId}/Comments/{versionId}/Delete/{commentId}")]
        [HttpPost("/Plugins/Edit/{pluginId}/Comments/Delete/{commentId}")]
        public async Task<IActionResult> Delete(int commentId, int pluginId, string versionId)
        {
            await _commentsRepository.DeleteComment(commentId, pluginId, versionId);
            TempData["StatusMessage"] = "Success! Comment was deleted!";
            return Content(null);
        }
    }
}

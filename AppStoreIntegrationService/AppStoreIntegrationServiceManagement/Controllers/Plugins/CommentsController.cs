using AppStoreIntegrationServiceCore.Repository.Interface;
using AppStoreIntegrationServiceCore.Model;
using Microsoft.AspNetCore.Mvc;

namespace AppStoreIntegrationServiceManagement.Controllers.Plugins
{
    [Area("Plugins")]
    public class CommentsController : Controller
    {
        private readonly ICommentsRepository _commentsRepository;
        private readonly IPluginRepository _pluginRepository;

        public CommentsController(ICommentsRepository commentsRepository, IPluginRepository pluginRepository)
        {
            _commentsRepository = commentsRepository;
            _pluginRepository = pluginRepository;
        }

        [HttpPost("/Plugins/Edit/{pluginId}/Comments/{versionId}/New")]
        public async Task<IActionResult> New(int pluginId, string versionId)
        {
            var plugin = await _pluginRepository.GetPluginById(pluginId);
            var comments = await _commentsRepository.GetCommentsForVersion(plugin.Name, versionId);
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
        public async Task<IActionResult> Update(Comment comment, int pluginId, string versionId)
        {
            var plugin = await _pluginRepository.GetPluginById(pluginId);
            await _commentsRepository.SaveComment(comment, plugin.Name, versionId);
            TempData["StatusMessage"] = "Success! Comment was added!";
            return Content(null);
        }

        [HttpPost("/Plugins/Edit/{pluginId}/Comments/{versionId}/Delete/{commentId}")]
        public async Task<IActionResult> Delete(int commentId, int pluginId, string versionId)
        {
            var plugin = await _pluginRepository.GetPluginById(pluginId);
            await _commentsRepository.DeleteVersionComment(commentId, plugin.Name, versionId);
            TempData["StatusMessage"] = "Success! Comment was added!";
            return Content(null);
        }
    }
}

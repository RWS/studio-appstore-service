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

        [HttpPost("/Plugins/Edit/{pluginId}/Comments/{versionId}")]
        public async Task<IActionResult> Show(int pluginId, string versionId)
        {
            var plugin = await _pluginRepository.GetPluginById(pluginId);

            await _commentsRepository.SaveComment(new Comment
            {
                CommentAuthor = "Catalin",
                CommentId = 0,
                CommentDate = DateTime.Now,
                CommentDescription = "Just a simple test"
            }, plugin.Name, versionId);
            
            var comments = await _commentsRepository.GetCommentsForVersion(plugin.Name, versionId);
            return PartialView("_CommentsPartial", (pluginId, versionId, comments));
        }
    }
}

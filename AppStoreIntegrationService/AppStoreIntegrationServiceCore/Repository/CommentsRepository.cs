using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using System.Xml.Linq;

namespace AppStoreIntegrationServiceCore.Repository
{
    public class CommentsRepository : ICommentsRepository
    {
        private readonly ICommentsManager _commentsManager;

        public CommentsRepository(ICommentsManager commentsManager) 
        {
            _commentsManager = commentsManager;
        }

        public async Task SaveComment(Comment comment, string pluginName)
        {
            var comments = await _commentsManager.ReadComments();
            if (!comments.TryAdd(pluginName, new List<Comment> { comment }))
            {
                _ = comments.TryGetValue(pluginName, out IEnumerable<Comment> pluginComments);
                if (pluginComments.Any(c => c.CommentId == comment.CommentId))
                {
                    pluginComments = pluginComments.Where(c => c.CommentId != comment.CommentId);
                }

                comments[pluginName] = pluginComments.Append(comment);
            }
            
            await _commentsManager.UpdateComments(comments);
        }

        public async Task DeleteComment(int id, string pluginName)
        {
            var comments = await _commentsManager.ReadComments();
            _ = comments.TryGetValue(pluginName, out IEnumerable<Comment> pluginComments);
            comments[pluginName] = pluginComments.Where(c => c.CommentId != id);
            await _commentsManager.UpdateComments(comments);
        }

        public async Task<IEnumerable<Comment>> GetCommentsForPlugin(string pluginName)
        {
            _ = (await _commentsManager.ReadComments()).TryGetValue(pluginName, out IEnumerable<Comment> comments);
            return comments ?? new List<Comment>();
        }

        public async Task<Comment> GetPluginComment(string pluginName, int id)
        {
            var comments = await _commentsManager.ReadComments();
            return comments[pluginName].FirstOrDefault(c => c.CommentId == id);
        }
    }
}

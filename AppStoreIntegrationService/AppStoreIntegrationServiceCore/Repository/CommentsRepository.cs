using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;

namespace AppStoreIntegrationServiceCore.Repository
{
    public class CommentsRepository : ICommentsRepository
    {
        private readonly ICommentsManager _commentsManager;

        public CommentsRepository(ICommentsManager commentsManager) 
        {
            _commentsManager = commentsManager;
        }

        public Task DeleteVersionComment(int id, string pluginName, string versionId)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Comment>> GetCommentsForVersion(string pluginName, string versionId)
        {
            var comments = await _commentsManager.ReadComments();

            if (comments.TryGetValue(pluginName, out var pluginComments))
            {
                if (pluginComments.TryGetValue(versionId, out var versionComments))
                {
                    return versionComments;
                }
            }

            return new List<Comment>();
        }

        public Task<Comment> GetVersionComment(string pluginName, int id, string versionId)
        {
            throw new NotImplementedException();
        }

        public async Task SaveComment(Comment comment, string pluginName, string versionId)
        {
            var comments = await _commentsManager.ReadComments();

            if (comments.TryGetValue(pluginName, out var pluginComments))
            {
                if (pluginComments.TryGetValue(versionId, out var versionComments))
                {
                    pluginComments[versionId] = versionComments.Append(comment);
                    comments[pluginName][versionId] = versionComments;
                }
                else
                {
                    pluginComments.Add(versionId, new[] { comment });
                    comments[pluginName] = pluginComments;
                }
            }
            else
            {
                comments.Add(pluginName, new Dictionary<string, IEnumerable<Comment>> { [versionId] = new[] { comment } });
            }
            
            await _commentsManager.UpdateComments(comments);
        }
    }
}

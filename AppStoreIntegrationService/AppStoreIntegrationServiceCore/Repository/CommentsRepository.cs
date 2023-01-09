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

        public async Task DeleteComment(int id, string pluginName, string versionId = null)
        {
            var comments = await _commentsManager.ReadComments();

            if (comments.TryGetValue(pluginName, out var package))
            {
                if (Equals(versionId, null))
                {
                    comments[pluginName].PluginComments = package.PluginComments.Where(c => !c.CommentId.Equals(id));
                }
                else if (package.VersionComments.TryGetValue(versionId, out var versionComments))
                {
                    comments[pluginName].VersionComments[versionId] = versionComments.Where(c => !c.CommentId.Equals(id));
                }
            }

            await _commentsManager.UpdateComments(comments);
        }

        public async Task<Comment> GetComment(string pluginName, int id, string versionId = null)
        {
            var comments = await _commentsManager.ReadComments();

            if (comments.TryGetValue(pluginName, out var package))
            {
                if (Equals(versionId, null))
                {
                    return package.PluginComments?.FirstOrDefault(c => c.CommentId.Equals(id));
                }

                if (package.VersionComments.TryGetValue(versionId, out var versionComments))
                {
                    return versionComments.FirstOrDefault(c => c.CommentId.Equals(id));
                }
            }

            return null;
        }

        public async Task<IEnumerable<Comment>> GetComments(string pluginName, string versionId = null)
        {
            var comments = await _commentsManager.ReadComments();

            if (comments.TryGetValue(pluginName, out var package))
            {
                if (Equals(versionId, null))
                {
                    return package.PluginComments ?? new List<Comment>();
                }

                if (package.VersionComments?.TryGetValue(versionId, out var versionComments) ?? false)
                {
                    return versionComments ?? new List<Comment>();
                }
            }

            return new List<Comment>();
        }

        private async Task SavePluginComment(Comment comment, string pluginName)
        {
            var comments = await _commentsManager.ReadComments();

            if (comments.TryGetValue(pluginName, out var package))
            {
                comments[pluginName].PluginComments = Update((package.PluginComments ?? new List<Comment>()).ToList(), comment);
            }
            else
            {
                comments.Add(pluginName, new CommentPackage { PluginComments = new List<Comment> { comment } });
            }

            await _commentsManager.UpdateComments(comments);
        }

        public async Task SaveComment(Comment comment, string pluginName, string versionId = null)
        {
            if (Equals(versionId, null))
            {
                await SavePluginComment(comment, pluginName);
            }
            else
            {
                await SaveVersionComment(comment, pluginName, versionId);
            }
        }

        private async Task SaveVersionComment(Comment comment, string pluginName, string versionId)
        {
            var comments = await _commentsManager.ReadComments();

            if (comments.TryGetValue(pluginName, out var package))
            {
                if ((package.VersionComments ?? new Dictionary<string, IEnumerable<Comment>>()).TryGetValue(versionId, out var versionComments))
                {
                    comments[pluginName].VersionComments[versionId] = Update(versionComments.ToList(), comment);
                }
                else
                {
                    comments[pluginName].VersionComments.Add(versionId, new List<Comment> { comment });
                }
            }
            else
            {
                comments.Add(pluginName, new CommentPackage
                {
                    VersionComments = new Dictionary<string, IEnumerable<Comment>>
                    {
                        [versionId] = new List<Comment> { comment }
                    }
                });
            }

            await _commentsManager.UpdateComments(comments);
        }

        private static List<Comment> Update(List<Comment> comments, Comment comment)
        {
            var index = comments?.IndexOf(comments.FirstOrDefault(c => c.CommentId.Equals(comment.CommentId))) ?? -1;

            if (index >= 0)
            {
                comments[index] = comment;
            }
            else
            {
                comments.Add(comment);
            }

            return comments;
        }

        public async Task DeleteComments(string pluginName, string versionId = null)
        {
            var comments = await _commentsManager.ReadComments();

            if (comments.TryGetValue(pluginName, out var package))
            {
                if (Equals(versionId, null))
                {
                    comments.Remove(pluginName);
                }
                else if (package.VersionComments.ContainsKey(versionId))
                {
                    comments[pluginName].VersionComments.Remove(versionId);
                }
            }

            await _commentsManager.UpdateComments(comments);
        }
    }
}

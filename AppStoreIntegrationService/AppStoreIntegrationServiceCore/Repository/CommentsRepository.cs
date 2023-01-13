﻿using AppStoreIntegrationServiceCore.Model;
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

        public async Task DeleteComment(int commentId, int pluginId, string versionId = null)
        {
            var comments = await _commentsManager.ReadComments();

            if (comments.TryGetValue(pluginId, out var package))
            {
                if (Equals(versionId, null))
                {
                    comments[pluginId].PluginComments = package.PluginComments.Where(c => !c.CommentId.Equals(commentId));
                }
                else if (package.VersionComments.TryGetValue(versionId, out var versionComments))
                {
                    comments[pluginId].VersionComments[versionId] = versionComments.Where(c => !c.CommentId.Equals(commentId));
                }
            }

            await _commentsManager.UpdateComments(comments);
        }

        public async Task<Comment> GetComment(int pluginId, int commentId, string versionId = null)
        {
            var comments = await _commentsManager.ReadComments();

            if (comments.TryGetValue(pluginId, out var package))
            {
                if (Equals(versionId, null))
                {
                    return package.PluginComments?.FirstOrDefault(c => c.CommentId.Equals(commentId));
                }

                if (package.VersionComments.TryGetValue(versionId, out var versionComments))
                {
                    return versionComments.FirstOrDefault(c => c.CommentId.Equals(commentId));
                }
            }

            return null;
        }

        public async Task<IEnumerable<Comment>> GetComments(int pluginId, string versionId = null)
        {
            var comments = await _commentsManager.ReadComments();

            if (comments.TryGetValue(pluginId, out var package))
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

        private async Task SavePluginComment(Comment comment, int pluginId)
        {
            var comments = await _commentsManager.ReadComments();

            if (comments.TryGetValue(pluginId, out var package))
            {
                comments[pluginId].PluginComments = Update((package.PluginComments ?? new List<Comment>()).ToList(), comment);
            }
            else
            {
                comments.Add(pluginId, new CommentPackage { PluginComments = new List<Comment> { comment } });
            }

            await _commentsManager.UpdateComments(comments);
        }

        public async Task SaveComment(Comment comment, int pluginId, string versionId = null)
        {
            if (Equals(versionId, null))
            {
                await SavePluginComment(comment, pluginId);
            }
            else
            {
                await SaveVersionComment(comment, pluginId, versionId);
            }
        }

        private async Task SaveVersionComment(Comment comment, int pluginId, string versionId)
        {
            var comments = await _commentsManager.ReadComments();

            if (comments.TryGetValue(pluginId, out var package))
            {
                if ((package.VersionComments ?? new Dictionary<string, IEnumerable<Comment>>()).TryGetValue(versionId, out var versionComments))
                {
                    comments[pluginId].VersionComments[versionId] = Update(versionComments.ToList(), comment);
                }
                else
                {
                    comments[pluginId].VersionComments.Add(versionId, new List<Comment> { comment });
                }
            }
            else
            {
                comments.Add(pluginId, new CommentPackage
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

        public async Task DeleteComments(int pluginId, string versionId = null)
        {
            var comments = await _commentsManager.ReadComments();

            if (comments.TryGetValue(pluginId, out var package))
            {
                if (Equals(versionId, null))
                {
                    comments.Remove(pluginId);
                }
                else if (package.VersionComments.ContainsKey(versionId))
                {
                    comments[pluginId].VersionComments.Remove(versionId);
                }
            }

            await _commentsManager.UpdateComments(comments);
        }
    }
}

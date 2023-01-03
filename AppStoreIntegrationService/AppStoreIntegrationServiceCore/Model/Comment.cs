using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace AppStoreIntegrationServiceCore.Model
{
    public class Comment : IEquatable<Comment>
    {
        public int CommentId { get; set; }
        public string CommentDescription { get; set; }
        public string CommentAuthor { get; set; }
        [Required]
        public DateTime CommentDate { get; set; }
        [JsonIgnore]
        public bool IsEditMode { get; set; }
        public int PluginId { get; set; }
        public string VersionId { get; set; }

        public bool Equals(Comment other)
        {
            return Equals(CommentDescription, other.CommentDescription);
        }
    }
}

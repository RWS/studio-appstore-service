using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace AppStoreIntegrationServiceCore.Model
{
    public class Comment
    {
        public int CommentId { get; set; }
        public string CommentDescription { get; set; }
        public string CommentAuthor { get; set; }
        [Required]
        public DateTime CommentDate { get; set; }
        [JsonIgnore]
        public bool IsEditMode { get; set; }
        [JsonIgnore]
        public int PluginId { get; set; }
        [JsonIgnore]
        public string VersionId { get; set; }
    }
}

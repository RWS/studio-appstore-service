using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace AppStoreIntegrationServiceCore.Model
{
    public class Comment
    {
        public int CommentId { get; set; }
        [JsonIgnore]
        public bool IsEditMode { get; set; }
        public string CommentAuthor { get; set; }
        [Required]
        public string CommentDescription { get; set; }
        public DateTime CommentDate { get; set; }
    }
}

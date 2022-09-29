using System.ComponentModel.DataAnnotations;

namespace AppStoreIntegrationServiceCore.Model
{
    public class NameMapping
    {
        public string Id { get; set; }

        [Required]
        public string NewName { get; set; }

        [Required]
        public string OldName { get; set; }
    }
}

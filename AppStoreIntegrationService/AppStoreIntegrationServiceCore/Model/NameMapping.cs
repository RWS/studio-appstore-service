using System.ComponentModel.DataAnnotations;

namespace AppStoreIntegrationServiceCore.Model
{
    public class NameMapping
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "New name is required!")]
        public string NewName { get; set; }

        [Required(ErrorMessage = "Old name is required!")]
        public string OldName { get; set; }
    }
}

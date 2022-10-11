using System.ComponentModel.DataAnnotations;

namespace AppStoreIntegrationServiceManagement.Model.Settings
{
    public class ImportPluginsModel
    {
        [Required]
        public IFormFile ImportedFile { get; set; }
    }
}

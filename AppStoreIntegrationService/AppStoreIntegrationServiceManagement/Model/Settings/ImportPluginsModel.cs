using System.ComponentModel.DataAnnotations;

namespace AppStoreIntegrationServiceManagement.Model.Settings
{
    public class ImportPluginsModel
    {
        [Required(ErrorMessage = "Plugins file is required!")]
        public IFormFile ImportedFile { get; set; }
    }
}

using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace AppStoreIntegrationServiceCore.Model
{
    public class ImportManifestModel
    {
        [Required(ErrorMessage = "Manifest is required!")]
        public IFormFile ManifestFile { get; set; }
    }
}

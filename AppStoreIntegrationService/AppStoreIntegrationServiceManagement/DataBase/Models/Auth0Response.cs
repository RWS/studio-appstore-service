using System.Net;

namespace AppStoreIntegrationServiceManagement.DataBase.Models
{
    public class Auth0Response
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Message { get; set; }
    }
}

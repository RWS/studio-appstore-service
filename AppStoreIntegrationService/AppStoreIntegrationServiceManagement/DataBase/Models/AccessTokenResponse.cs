using Newtonsoft.Json;

namespace AppStoreIntegrationServiceManagement.DataBase.Models
{
    public class AccessTokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("token_type")]
        public string TokenType { get; set; }
    }
}

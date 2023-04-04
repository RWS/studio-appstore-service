using AppStoreIntegrationServiceCore.DataBase.Models;
using AppStoreIntegrationServiceCore.ExtensionMethods;
using AppStoreIntegrationServiceManagement.Helpers;
using AppStoreIntegrationServiceManagement.Model.Identity.Interface;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace AppStoreIntegrationServiceManagement.Model.Identity
{
    public class Auth0UserManager : IAuth0UserManager
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public Auth0UserManager(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _configuration = configuration;
        }

        public async Task<IEnumerable<UserProfile>> GetUsers()
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAccessToken());
            var response = await _httpClient.GetAsync($"https://{_configuration["Auth0:Domain"]}{_configuration["Auth0:ApiPath"]}users");

            var responseBody = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<IEnumerable<UserProfile>>(responseBody);
        }

        public async Task<UserProfile> GetUserById(string id)
        {
            var users = await GetUsers();
            return users.FirstOrDefault(x => x.UserId == id);
        }

        public async Task<Auth0Response> TryCreateUser(RegisterModel model)
        {
            string url = $"https://{_configuration["Auth0:Domain"]}/dbconnections/signup";
            var content = JsonConvert.SerializeObject(new
            {
                client_id = _configuration["Auth0:ClientId"],
                email = model.Email,
                password = model.Password,
                username = model.Username,
                connection = "Username-Password-Authentication"
            });

            try
            {
                var response = await _httpClient.PostAsync(url, new StringContent(content, Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    return new Auth0Response { StatusCode = response.StatusCode };
                }

                return JsonConvert.DeserializeObject<Auth0Response>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                return new Auth0Response
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = ex.Message
                };
            }
        }

        public async Task<Auth0Response> TryUpdateUserEmail(string userId, string email)
        {
            return await TryUpdate(userId, JsonConvert.SerializeObject(new { email, name = email }));
        }

        private async Task<Auth0Response> TryUpdate(string userId, string content)
        {
            string url = $"https://{_configuration["Auth0:Domain"]}{_configuration["Auth0:ApiPath"]}users/{userId}";

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAccessToken());
                var response = await _httpClient.PatchAsync(url, new StringContent(content, Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    return new Auth0Response { StatusCode = response.StatusCode };
                }

                return JsonConvert.DeserializeObject<Auth0Response>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                return new Auth0Response
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = ex.Message
                };
            }
        }

        private async Task<string> GetAccessToken()
        {
            var query = new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["audience"] = $"https://{_configuration["Auth0:Domain"]}{_configuration["Auth0:ApiPath"]}",
                ["client_id"] = _configuration["Auth0:ClientId"],
                ["client_secret"] = _configuration["Auth0:ClientSecret"]
            };

            var content = new StringContent(query.ToQuery(), Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = await _httpClient.PostAsync($"https://{_configuration["Auth0:Domain"]}/oauth/token", content);

            return JsonConvert.DeserializeObject<AccessTokenResponse>(await response.Content.ReadAsStringAsync()).AccessToken;
        }
    }
}

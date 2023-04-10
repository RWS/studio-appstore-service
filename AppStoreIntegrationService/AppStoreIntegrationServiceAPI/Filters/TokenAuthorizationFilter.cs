using AppStoreIntegrationServiceCore.DataBase.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text;

namespace AppStoreIntegrationServiceAPI.Filters
{
    public class TokenAuthorizationFilter : IAsyncAuthorizationFilter
    {
        private readonly IUserProfilesManager _userProfilesManager;
        private readonly IConfiguration _configuration;

        public TokenAuthorizationFilter(IUserProfilesManager userProfilesManager, IConfiguration configuration)
        {
            _userProfilesManager = userProfilesManager;
            _configuration = configuration;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var authorization = context.HttpContext.Request.Headers.Authorization;
            _ = AuthenticationHeaderValue.TryParse(authorization, out var parsedValue);

            if (parsedValue == null || parsedValue.Scheme != "Bearer" || string.IsNullOrEmpty(parsedValue.Parameter))
            {
                context.Result = new UnauthorizedResult();
            }

            if (IsValidStaticToken(parsedValue.Parameter) || await IsValidJWTToken(parsedValue.Parameter))
            {
                return;
            }

            context.Result = new UnauthorizedResult();
        }

        private async Task<bool> IsValidJWTToken(string parameter)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>($"https://{_configuration["Auth0:Domain"]}/.well-known/openid-configuration", new OpenIdConnectConfigurationRetriever());
            var openIdConfig = await configurationManager.GetConfigurationAsync(CancellationToken.None);
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = $"https://{_configuration["Auth0:Domain"]}/",
                ValidateAudience = true,
                ValidAudience = $"https://{_configuration["Auth0:Domain"]}{_configuration["Auth0:ApiPath"]}",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                IssuerSigningKey = openIdConfig.SigningKeys.FirstOrDefault()
            };

            try
            {
                _ = tokenHandler.ValidateToken(parameter, validationParameters, out var validatedToken);
                return true;
            }
            catch (SecurityTokenException)
            {
                return false;
            }

        }

        private bool IsValidStaticToken(string parameter)
        {
            var users = _userProfilesManager.UserProfiles;
            return users.Any(x => x.APIAccessToken == parameter);
        }
    }
}

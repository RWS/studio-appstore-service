using AppStoreIntegrationServiceCore.DataBase;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net.Http.Headers;

namespace AppStoreIntegrationServiceAPI.Filters
{
    public class TokenAuthorizationFilter : IAuthorizationFilter
    {
        private readonly UserManager<IdentityUserExtended> _userManager;

        public TokenAuthorizationFilter(UserManager<IdentityUserExtended> userManager)
        {
            _userManager = userManager;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var authorization = context.HttpContext.Request.Headers.Authorization;
            _ = AuthenticationHeaderValue.TryParse(authorization, out var parsedValue);

            if (parsedValue == null || parsedValue.Scheme != "Bearer" || string.IsNullOrEmpty(parsedValue.Parameter))
            {
                context.Result = new UnauthorizedResult();
            }

            var users = _userManager.Users.ToList();

            if (users.Any(x => x.APIAccessToken == parsedValue.Parameter))
            {
                return;
            }

            context.Result = new UnauthorizedResult();
        }
    }
}

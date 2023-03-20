using Microsoft.AspNetCore.Mvc;

namespace AppStoreIntegrationServiceAPI.Filters
{
    public class TokenAuthorizationAttribute : TypeFilterAttribute
    {
        public TokenAuthorizationAttribute() : base(typeof(TokenAuthorizationFilter)) { }
    }
}

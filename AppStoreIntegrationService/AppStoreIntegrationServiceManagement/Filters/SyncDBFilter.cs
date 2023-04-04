using AppStoreIntegrationServiceCore.DataBase.Interface;
using AppStoreIntegrationServiceManagement.Model.Identity.Interface;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace AppStoreIntegrationServiceManagement.Filters
{
    public class SyncDBFilter : IAsyncAuthorizationFilter
    {
        private readonly IUserProfilesManager _userProfilesManager;
        private readonly IAuth0UserManager _auth0UserManager;

        public SyncDBFilter(IUserProfilesManager userProfilesManager, IAuth0UserManager auth0UserManager)
        {
            _userProfilesManager = userProfilesManager;
            _auth0UserManager = auth0UserManager;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var principal = context.HttpContext.User;
            var user = _userProfilesManager.GetUser(principal);
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (user != null)
            {
                var auth0User = await _auth0UserManager.GetUserById(userId);
                _userProfilesManager.UpdateUserName(user, auth0User.Name);
                _userProfilesManager.UpdateUserId(user, userId);
                return;
            }

            var returnUrl = context.HttpContext.Request.Path.Value;
            context.HttpContext.Response.Redirect($"~/AccessDenied?{returnUrl}");
        }
    }
}

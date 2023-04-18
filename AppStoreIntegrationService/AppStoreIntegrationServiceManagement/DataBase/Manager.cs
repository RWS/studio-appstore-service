using Microsoft.AspNetCore.Identity;

namespace AppStoreIntegrationServiceManagement.DataBase
{
    public class Manager
    {
        protected bool ExistNullParams(out IdentityResult result, params string[] values)
        {
            foreach (var item in values)
            {
                if (string.IsNullOrEmpty(item))
                {
                    result = IdentityResult.Failed(new IdentityError
                    {
                        Description = $"Parameter cannot be null: {nameof(item)}"
                    });

                    return false;
                }
            }

            result = null;
            return true;
        }
    }
}
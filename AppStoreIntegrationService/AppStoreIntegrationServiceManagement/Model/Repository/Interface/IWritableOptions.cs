using Microsoft.Extensions.Options;

namespace AppStoreIntegrationServiceManagement.Model.Repository.Interface
{
    public interface IWritableOptions<TOptions> : IOptions<TOptions> where TOptions : class, new()
    {
        void SaveOption(TOptions options);
    }
}

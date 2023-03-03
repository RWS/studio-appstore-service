using Microsoft.Extensions.Options;

namespace AppStoreIntegrationServiceManagement.Repository.Interface
{
    public interface IWritableOptions<TOptions> : IOptions<TOptions> where TOptions : class, new()
    {
        void SaveOption(TOptions options);
    }
}

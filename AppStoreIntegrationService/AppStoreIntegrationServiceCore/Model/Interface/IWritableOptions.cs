using Microsoft.Extensions.Options;

namespace AppStoreIntegrationServiceCore.Model.Common.Interface
{
    public interface IWritableOptions<TOptions> : IOptions<TOptions> where TOptions : class, new()
    {
        void SaveOption(TOptions options);
    }
}

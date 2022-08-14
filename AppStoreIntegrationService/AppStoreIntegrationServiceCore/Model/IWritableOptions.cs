using Microsoft.Extensions.Options;

namespace AppStoreIntegrationServiceCore.Model
{
    public interface IWritableOptions<TOptions> : IOptions<TOptions> where TOptions : class, new()
    {
        void SaveOption(TOptions options);
    } 
}

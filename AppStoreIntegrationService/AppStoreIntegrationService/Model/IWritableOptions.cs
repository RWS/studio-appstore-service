using Microsoft.Extensions.Options;

namespace AppStoreIntegrationService.Model
{
    public interface IWritableOptions<TOptions> : IOptions<TOptions> where TOptions : class, new()
    {
        void SaveOption(TOptions options);
    } 
}

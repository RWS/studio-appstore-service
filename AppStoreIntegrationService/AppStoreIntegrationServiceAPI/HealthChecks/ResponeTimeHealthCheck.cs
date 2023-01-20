using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;

namespace AppStoreIntegrationServiceAPI.HealthChecks
{
    public class ResponeTimeHealthCheck : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var watch = new Stopwatch();

            try
            {
                watch.Start();
                var client = new HttpClient();
                var message = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri("https://localhost:44375/plugins")
                };

                var downloadResponse = await client.SendAsync(message);
                _ = await downloadResponse.Content.ReadAsStringAsync();
                watch.Stop();
            }
            catch (Exception)
            {
                return HealthCheckResult.Unhealthy("Couldn't get the response!");
            }

            if (watch.ElapsedMilliseconds > 100)
            {
                return HealthCheckResult.Unhealthy($"The response time for this endpoint is {watch.ElapsedMilliseconds} ms!");
            }

            if (watch.ElapsedMilliseconds > 10)
            {
                return HealthCheckResult.Degraded($"The response time for this endpoint is {watch.ElapsedMilliseconds} ms!");
            }

            return HealthCheckResult.Healthy($"The response time for this endpoint is {watch.ElapsedMilliseconds} ms!");
        }
    }
}

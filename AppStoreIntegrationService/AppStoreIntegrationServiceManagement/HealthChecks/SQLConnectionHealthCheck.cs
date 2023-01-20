using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Data.Common;

namespace AppStoreIntegrationServiceManagement.HealthChecks
{
    public class SQLConnectionHealthCheck : IHealthCheck
    {
        public string _connectionString;

        public SQLConnectionHealthCheck(IConfiguration configuration)
        {
            _connectionString = configuration.GetSection("ConnectionStrings:AppStoreIntegrationServiceContextConnection").Value;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    await connection.OpenAsync(cancellationToken);
                    var command = connection.CreateCommand();
                    command.CommandText = "select 1";
                    await command.ExecuteNonQueryAsync(cancellationToken);

                }
                catch (DbException ex)
                {
                    return new HealthCheckResult(status: context.Registration.FailureStatus, exception: ex);
                }
            }

            return HealthCheckResult.Healthy();
        }
    }
}

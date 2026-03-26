using Microsoft.Extensions.Diagnostics.HealthChecks;
using AdPhotoManager.Core.Interfaces;

namespace AdPhotoManager.Api.HealthChecks;

public class AdConnectionHealthCheck : IHealthCheck
{
    private readonly ILdapConnection _ldapConnection;

    public AdConnectionHealthCheck(ILdapConnection ldapConnection)
    {
        _ldapConnection = ldapConnection;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var isConnected = _ldapConnection.TestConnection();

            if (isConnected)
            {
                return await Task.FromResult(
                    HealthCheckResult.Healthy("AD connection is healthy"));
            }

            return await Task.FromResult(
                HealthCheckResult.Degraded("AD connection test failed"));
        }
        catch (Exception ex)
        {
            return await Task.FromResult(
                HealthCheckResult.Unhealthy("AD connection failed", ex));
        }
    }
}

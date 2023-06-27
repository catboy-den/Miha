using TinyHealthCheck.HealthChecks;
using TinyHealthCheck.Models;

namespace MidnightHaven.Chan.Health;

public class LivenessHealthCheck : IHealthCheck
{
    public LivenessHealthCheck()
    {

    }

    public async Task<IHealthCheckResult> ExecuteAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

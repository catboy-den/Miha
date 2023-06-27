using System.Globalization;
using System.Net;
using Discord.WebSocket;
using Redis.OM.Contracts;
using TinyHealthCheck.HealthChecks;
using TinyHealthCheck.Models;

namespace MidnightHaven.Chan.Health;

public class ReadinessHealthCheck : IHealthCheck
{
    private readonly DiscordSocketClient _client;
    private readonly IRedisConnectionProvider _provider;

    public ReadinessHealthCheck(
        DiscordSocketClient client,
        IRedisConnectionProvider provider)
    {
        _client = client;
        _provider = provider;
    }

    public Task<IHealthCheckResult> ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            var s = _provider.Connection.Execute("PING").ToString(CultureInfo.InvariantCulture);
            var c = _client.ConnectionState;

            return Task.FromResult<IHealthCheckResult>(new JsonHealthCheckResult( new { discord = c, redis = s}, HttpStatusCode.OK));
        }
        catch (Exception)
        {

        }

        return Task.FromResult<IHealthCheckResult>(new JsonHealthCheckResult( new { }, HttpStatusCode.Locked));
    }
}

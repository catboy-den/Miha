using System.Globalization;
using System.Net;
using Discord;
using Discord.WebSocket;
using Redis.OM.Contracts;
using StackExchange.Redis;
using TinyHealthCheck.HealthChecks;
using TinyHealthCheck.Models;

namespace MidnightHaven.Chan.Health;

public class MihaHealthCheck : IHealthCheck
{
    private readonly DiscordSocketClient _client;
    private readonly IRedisConnectionProvider _provider;

    public MihaHealthCheck(
        DiscordSocketClient client,
        IRedisConnectionProvider provider)
    {
        _client = client;
        _provider = provider;
    }

    public async Task<IHealthCheckResult> ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            var redisPing = await _provider.Connection.ExecuteAsync("PING");
            var discordState = _client.ConnectionState;

            var discord = discordState.ToString();
            var redis = redisPing.ToString(CultureInfo.InvariantCulture);

            var healthy = discordState == ConnectionState.Connected && redis == "PONG";
            var status = healthy ? "healthy" : "un-healthy";
            var statusCode = healthy ? HttpStatusCode.OK : HttpStatusCode.Locked;

            return new JsonHealthCheckResult(new
            {
                discord,
                redis,
                status
            }, statusCode);
        }
        catch (RedisTimeoutException)
        {
            return new JsonHealthCheckResult(new
            {
                status = "redis timed out"
            }, HttpStatusCode.Locked);
        }
    }
}

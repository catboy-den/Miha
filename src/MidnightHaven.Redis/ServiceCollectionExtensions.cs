using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MidnightHaven.Redis.Repositories;
using MidnightHaven.Redis.Repositories.Interfaces;
using MidnightHaven.Redis.Services;
using Redis.OM;
using Redis.OM.Contracts;

namespace MidnightHaven.Redis;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
        var redisOptions = configuration.GetSection(RedisOptions.Section);

        services.AddOptions<RedisOptions>().Bind(redisOptions);
        services.AddSingleton<IRedisConnectionProvider>(provider =>
        {
            var redisConnectionConfig = provider.GetRequiredService<IOptions<RedisOptions>>().Value;

            return new RedisConnectionProvider(new RedisConnectionConfiguration
            {
                Host = redisConnectionConfig.Host,
                Port = redisConnectionConfig.Port,
                Password = null
            });
        });

        services.AddHostedService<IndexCreationService>();

        services.AddRedisRepositories();

        return services;
    }

    private static IServiceCollection AddRedisRepositories(this IServiceCollection services)
    {
        services.AddSingleton<IGuildDocumentRepository, GuildDocumentRepository>();

        return services;
    }
}

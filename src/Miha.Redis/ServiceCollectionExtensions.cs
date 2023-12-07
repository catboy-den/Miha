using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Miha.Redis.Repositories;
using Miha.Redis.Repositories.Interfaces;
using Miha.Redis.Services;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using Redis.OM;
using Redis.OM.Contracts;

namespace Miha.Redis;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
        var redisOptions = configuration.GetSection(RedisOptions.Section);

        RedisSerializationSettings.JsonSerializerOptions.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
        
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

        services.AddHostedService<RedisIndexCreationService>();
        services.AddSingleton<RedisSeedService>();
        
        services.AddRedisRepositories();
        
        return services;
    }

    private static IServiceCollection AddRedisRepositories(this IServiceCollection services)
    {
        services.AddSingleton<IBirthdayJobRepository, BirthdayJobRepository>();
        services.AddSingleton<IGuildRepository, GuildRepository>();
        services.AddSingleton<IUserRepository, UserRepository>();

        return services;
    }
}

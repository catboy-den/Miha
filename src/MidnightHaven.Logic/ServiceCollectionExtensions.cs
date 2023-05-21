using Microsoft.Extensions.DependencyInjection;
using MidnightHaven.Logic.Services;
using MidnightHaven.Logic.Services.Interfaces;

namespace MidnightHaven.Logic;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLogicServices(this IServiceCollection services)
    {
        services.AddSingleton<IGuildService, GuildService>();
        services.AddSingleton<IUserService, UserService>();

        return services;
    }
}

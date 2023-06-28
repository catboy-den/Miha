using Microsoft.Extensions.DependencyInjection;
using Miha.Logic.Services;
using Miha.Logic.Services.Interfaces;

namespace Miha.Logic;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLogicServices(this IServiceCollection services)
    {
        services.AddSingleton<IBirthdayJobService, BirthdayJobService>();
        services.AddSingleton<IGuildService, GuildService>();
        services.AddSingleton<IUserService, UserService>();

        return services;
    }
}

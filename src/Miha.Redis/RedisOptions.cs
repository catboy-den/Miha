using Microsoft.Extensions.Configuration;

namespace Miha.Redis;

public class RedisOptions
{
    public const string Section = "Redis";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 6379;

    [ConfigurationKeyName("Seeding")] 
    public RedisSeedOptions SeedOptions { get; set; } = new();
}

public class RedisSeedOptions
{
    [ConfigurationKeyName("Guild")] 
    public RedisSeedGuildOptions? GuildOptions { get; set; }
}

public class RedisSeedGuildOptions
{
    public ulong? AnnouncementChannel { get; set; }
        
    public ulong? AnnouncementRoleId { get; set; }
        
    public ulong? BirthdayAnnouncementChannel { get; set; }
        
    public ulong? WeeklyScheduleChannel { get; set; }
        
    public ulong? LogChannel { get; set; }
}

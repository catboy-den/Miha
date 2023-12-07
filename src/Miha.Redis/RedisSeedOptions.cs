using Microsoft.Extensions.Configuration;

namespace Miha.Redis;

public class RedisSeedOptions
{
    public const string Section = "RedisSeed";

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

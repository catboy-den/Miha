using Redis.OM.Modeling;

namespace MidnightHaven.Redis.Models;

[Document(StorageType = StorageType.Json, Prefixes = new []{"guildOptions"})]
public class GuildOptions
{
    [Indexed]
    [RedisIdField]
    public ulong GuildId { get; set; }

    [Indexed]
    public ulong? AnnouncementChannel { get; set; }

    [Indexed]
    public ulong? AnnouncementRoleId { get; set; }

    [Indexed]
    public ulong? LogChannel { get; set; }
}

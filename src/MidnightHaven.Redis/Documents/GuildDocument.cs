using Redis.OM.Modeling;

namespace MidnightHaven.Redis.Documents;

[Document(StorageType = StorageType.Json, Prefixes = new []{"guild"})]
public class GuildDocument
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

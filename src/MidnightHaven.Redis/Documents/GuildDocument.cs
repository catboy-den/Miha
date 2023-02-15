using NodaTime;
using Redis.OM.Modeling;

namespace MidnightHaven.Redis.Documents;

[Document(StorageType = StorageType.Json, Prefixes = new[] { "guild" })]
public class GuildDocument : Document
{
    [Indexed]
    [RedisIdField]
    public override ulong Id { get; set; }

    [Indexed]
    public ulong GuildId => base.Id;

    [Indexed]
    public ulong? AnnouncementChannel { get; set; }

    [Indexed]
    public ulong? AnnouncementRoleId { get; set; }

    [Indexed]
    public ulong? LogChannel { get; set; }
}

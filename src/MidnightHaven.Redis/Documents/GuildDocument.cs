using System.Text.Json.Serialization;
using Redis.OM.Modeling;

namespace MidnightHaven.Redis.Documents;

[Document(StorageType = StorageType.Json, Prefixes = new[] { "guild" })]
public class GuildDocument : Document
{
    [Indexed]
    [RedisIdField]
    [JsonPropertyName("GuildId")]
    public override ulong Id { get; set; }

    [JsonIgnore]
    public ulong GuildId => base.Id;

    [Indexed]
    public ulong? AnnouncementChannel { get; set; }

    [Indexed]
    public ulong? AnnouncementRoleId { get; set; }

    [Indexed]
    public ulong? LogChannel { get; set; }
}

using System.Text.Json.Serialization;
using Redis.OM.Modeling;

namespace MidnightHaven.Redis.Documents;

[Document(StorageType = StorageType.Json, Prefixes = new []{"user"})]
public class UserDocument : Document
{
    [Indexed]
    [RedisIdField]
    [JsonPropertyName("UserId")]
    public override ulong Id { get; set; }

    [JsonIgnore]
    public ulong UserId => base.Id;

    [Indexed]
    public Guid? VrcUsrId { get; set; }
}

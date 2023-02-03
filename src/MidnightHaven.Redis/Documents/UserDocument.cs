using Redis.OM.Modeling;

namespace MidnightHaven.Redis.Documents;

[Document(StorageType = StorageType.Json, Prefixes = new []{"user"})]
public class UserDocument
{
    [Indexed]
    [RedisIdField]
    public ulong UserId { get; set; }

    [Indexed]
    public Guid? VrcUsrId { get; set; }
}

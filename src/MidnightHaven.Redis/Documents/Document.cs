using Redis.OM.Modeling;

namespace MidnightHaven.Redis.Documents;

public class Document
{
    [Indexed]
    [RedisIdField]
    public virtual ulong Id { get; set; }
}

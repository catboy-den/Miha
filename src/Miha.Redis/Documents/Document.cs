using Redis.OM.Modeling;

namespace Miha.Redis.Documents;

public class Document
{
    [Indexed]
    [RedisIdField]
    public virtual ulong Id { get; set; }
}

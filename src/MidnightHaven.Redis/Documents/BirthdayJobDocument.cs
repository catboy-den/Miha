using NodaTime;
using Redis.OM.Modeling;

namespace MidnightHaven.Redis.Documents;

[Document(StorageType = StorageType.Json, Prefixes = new[] { "birthdayJob" })]
public class BirthdayJobDocument : Document
{
    [Indexed]
    public ulong UserDocumentId { get; set; }

    [Indexed]
    public LocalDate BirthdayDate { get; set; }
}

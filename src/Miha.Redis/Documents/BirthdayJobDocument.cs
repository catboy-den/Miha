using NodaTime;
using Redis.OM.Modeling;

namespace Miha.Redis.Documents;

[Document(StorageType = StorageType.Json, Prefixes = new[] { "birthdayJob" })]
public class BirthdayJobDocument : Document
{
    [Indexed]
    public ulong UserId { get; set; }

    /// <summary>
    /// Users birthdate converted to EST
    /// </summary>
    [Indexed]
    public LocalDate BirthdayDate { get; set; }
}

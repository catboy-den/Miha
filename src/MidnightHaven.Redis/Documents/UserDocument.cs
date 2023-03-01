using NodaTime;
using Redis.OM.Modeling;

namespace MidnightHaven.Redis.Documents;

[Document(StorageType = StorageType.Json, Prefixes = new []{"user"})]
public class UserDocument : Document
{
    private const string VrcUsrUrl = "https://vrchat.com/home/user/";

    [Indexed]
    [RedisIdField]
    public override ulong Id { get; set; }

    [Indexed]
    public string? VrcUsrId { get; set; }

    [Indexed]
    public string? Timezone { get; set; }

    [Indexed]
    public AnnualDate? AnnualBirthdate { get; set; }

    [Indexed]
    public bool EnableBirthday { get; set; } = false;

    public string? GetVrcUsrUrl() => VrcUsrId is not null ? VrcUsrUrl + VrcUsrId : null;
    public string? GetHyperLinkedVrcUsrUrl(string? hyperLinkText = null)
    {
        var usrUrl = GetVrcUsrUrl();
        return usrUrl is not null ?  $"[{hyperLinkText ?? usrUrl}]({GetVrcUsrUrl()})" : null;
    }
}

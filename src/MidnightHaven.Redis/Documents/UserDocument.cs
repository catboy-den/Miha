using System.Text.Json.Serialization;
using Redis.OM.Modeling;

namespace MidnightHaven.Redis.Documents;

[Document(StorageType = StorageType.Json, Prefixes = new []{"user"})]
public class UserDocument : Document
{
    private const string VrcUsrUrl = "https://vrchat.com/home/user/";

    [Indexed]
    [RedisIdField]
    [JsonPropertyName("UserId")]
    public override ulong Id { get; set; }

    [JsonIgnore]
    public ulong UserId => base.Id;

    [Indexed]
    public string? VrcUsrId { get; set; }

    [Indexed]
    public string? IanaTimeZone { get; set; }

    public string? GetVrcUsrUrl() => VrcUsrId is not null ? VrcUsrUrl + VrcUsrId : null;
    public string? GetHyperLinkedVrcUsrUrl(string? hyperLinkText = null)
    {
        var usrUrl = GetVrcUsrUrl();
        return usrUrl is not null ?  $"[{hyperLinkText ?? usrUrl}]({GetVrcUsrUrl()})" : null;
    }
}

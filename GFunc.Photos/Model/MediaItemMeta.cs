using System.Text.Json.Serialization;

namespace GFunc.Photos.Model;

public class MediaItemMeta
{
    [JsonPropertyName("creationTime")]
    public DateTime CreationTimeUtc { get; }

    public MediaItemMeta(DateTime creationTimeUtc)
    {
        CreationTimeUtc = creationTimeUtc;
    }
}
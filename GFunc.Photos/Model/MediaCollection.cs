using System.Text.Json.Serialization;

namespace GFunc.Photos.Model;

public class MediaCollection
{
    [JsonPropertyName("mediaItems")]
    public List<MediaItem> Items { get; }

    public MediaCollection(List<MediaItem> items)
    {
        Items = items;
    }
}
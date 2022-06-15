using System.Text.Json.Serialization;

namespace GFunc.Photos.Model;

public class MediaCollection
{
    [JsonPropertyName("mediaItems")]
    public List<MediaItem> Items { get; }

    [JsonPropertyName("nextPageToken")]
    public string? NextPageToken { get; }

    public MediaCollection(List<MediaItem> items, string? nextPageToken)
    {
        Items = items;
        NextPageToken = nextPageToken;
    }
}
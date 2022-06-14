using System.Text.Json.Serialization;

namespace GFunc.Photos.Model;

public class MediaItem
{
    [JsonPropertyName("id")]
    public string Id { get; }
    
    [JsonPropertyName("baseUrl")]
    public string Url { get; }

    [JsonPropertyName("mimeType")]
    public string MimeType { get; }

    [JsonPropertyName("filename")]
    public string Filename { get; }
    
    [JsonPropertyName("mediaMetadata")]
    public MediaItemMeta Metadata { get; }

    public MediaItem(string id, string url, string mimeType, string filename, MediaItemMeta metadata)
    {
        Id = id;
        Url = url;
        MimeType = mimeType;
        Filename = filename;
        Metadata = metadata;
    }
}
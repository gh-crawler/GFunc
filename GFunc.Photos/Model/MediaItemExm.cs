namespace GFunc.Photos.Model;

public static class MediaItemExm
{
    public static bool IsVideo(this MediaItem item)
    {
        return item.MimeType.StartsWith("video", StringComparison.OrdinalIgnoreCase);
    }

    public static string BuildPath(this MediaItem item)
    {
        var date = item.Metadata.CreationTimeUtc;
        string ext = Path.GetExtension(item.Filename);
        return $"{date.Year}/{date.Month}/{date:yyyyMMddHHmmss}{ext}";
    }
}
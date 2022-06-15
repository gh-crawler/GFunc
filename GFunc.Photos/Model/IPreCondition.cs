using System.Text;

namespace GFunc.Photos.Model;

public interface IPreCondition
{
    void Apply(HttpRequestMessage request, string? nextPageToken);
}

public class AlbumCondition : IPreCondition
{
    private readonly string _albumId;

    public void Apply(HttpRequestMessage request, string? nextPageToken)
    {
        request.Content = string.IsNullOrEmpty(nextPageToken)
            ? new StringContent($"{{ \"pageSize\": 100, \"albumId\": \"{_albumId}\"}}", Encoding.Default, "application/json")
            : new StringContent($"{{ \"pageSize\": 100, \"albumId\": \"{_albumId}\", \"pageToken\": \"{nextPageToken}\"}}", Encoding.Default, "application/json");
    }

    public AlbumCondition(string albumId)
    {
        _albumId = albumId;
    }
}
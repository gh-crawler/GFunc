using System.Net.Http.Headers;
using System.Text.Json;
using GFunc.Photos.Model;

namespace GFunc.Photos;

public class GooglePhotosProvider : IMediaProvider
{
    private readonly string _accessToken;
    private readonly string _refreshToken;

    public async Task<IReadOnlyCollection<MediaItem>> GetMediaAsync(IReadOnlyCollection<IPreCondition> preConditions)
    {
        using var client = new HttpClient();

        string? nextPageToken = null;

        var result = new List<MediaItem>();

        do
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "https://photoslibrary.googleapis.com/v1/mediaItems:search");

            if (preConditions.Count > 0)
            {
                foreach (var condition in preConditions)
                    condition.Apply(request, nextPageToken);
            }

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            request.Headers.Accept.ParseAdd("application/json");

            using var response = await client.SendAsync(request);

            string json = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();

            var collection = JsonSerializer.Deserialize<MediaCollection>(json);

            if (collection == null)
                break;

            result.AddRange(collection.Items);
            nextPageToken = collection.NextPageToken;

        } while (!string.IsNullOrEmpty(nextPageToken));

        return result;
    }

    public GooglePhotosProvider(string accessToken, string refreshToken)
    {
        _accessToken = accessToken;
        _refreshToken = refreshToken;
    }
}
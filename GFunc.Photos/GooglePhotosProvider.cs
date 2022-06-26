using System.Net.Http.Headers;
using System.Text.Json;
using GFunc.Photos.Model;

namespace GFunc.Photos;

public class GooglePhotosProvider : IMediaProvider
{
    private readonly ITokenProvider _tokenProvider;

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

            var token = await _tokenProvider.GetTokenAsync();

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            request.Headers.Accept.ParseAdd("application/json");

            using var response = await client.SendAsync(request);

            string json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Failed to get media items. Response: {json}");

            var collection = JsonSerializer.Deserialize<MediaCollection>(json);

            if (collection == null)
                break;

            result.AddRange(collection.Items);
            nextPageToken = collection.NextPageToken;

        } while (!string.IsNullOrEmpty(nextPageToken));

        return result;
    }

    public GooglePhotosProvider(ITokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider;
    }
}
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GFunc.Photos;

public static class GoogleApiTokenClient
{
    private class Token
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; }
        
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; }
        
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; }

        public Token(string accessToken, string refreshToken, Int32 expiresIn)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            ExpiresIn = expiresIn;
        }
    }

    private const string TokenRequestUri = "https://www.googleapis.com/oauth2/v4/token";
    
    public static async Task<GoogleToken> ByCodeAsync(string code, string redirectUri, string clientId, string clientSecret, string scope)
    {
        string tokenRequestBody = $"code={code}&redirect_uri={redirectUri}&client_id={clientId}&client_secret={clientSecret}&scope={scope}&grant_type=authorization_code";

        var token = await InternalGetTokenAsync(tokenRequestBody);
        
        return new GoogleToken(token.AccessToken, token.RefreshToken, DateTime.UtcNow.AddSeconds(token.ExpiresIn - 300), scope, redirectUri);
    }

    public static async Task<GoogleToken> ByRefreshToken(GoogleToken googleToken, string clientId, string clientSecret)
    {
        string tokenRequestBody = $"refresh_token={googleToken.RefreshToken}&grant_type=refresh_token&client_id={clientId}&client_secret={clientSecret}";

        var token = await InternalGetTokenAsync(tokenRequestBody);
        
        return new GoogleToken(token.AccessToken, googleToken.RefreshToken, DateTime.UtcNow.AddSeconds(token.ExpiresIn - 300), googleToken.Scope, googleToken.RedirectUri);
    }

    private static async Task<Token> InternalGetTokenAsync(string tokenRequestBody)
    {
        using HttpClient client = new HttpClient();

        using var body = new StringContent(tokenRequestBody);
        body.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

        using var request = new HttpRequestMessage(HttpMethod.Post, TokenRequestUri);
        request.Headers.Accept.ParseAdd("text/html");
        request.Headers.Accept.ParseAdd("application/xhtml+xml");
        request.Headers.Accept.ParseAdd("application/xml");

        using var response = await client.PostAsync(TokenRequestUri, body);

        string responseText = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<Token>(responseText) ?? throw new Exception("Cannot deserialize token");
    }
}
using System.Text.Json.Serialization;

namespace GFunc.Photos;

public class GoogleToken
{
    public string AccessToken { get; }
    public string RefreshToken { get; }
    public DateTime ExpirationDateUtc { get; }
    public string Scope { get; }
    public string RedirectUri { get; }

    public bool IsExpired => DateTime.UtcNow > ExpirationDateUtc;

    public GoogleToken(string accessToken, string refreshToken, DateTime expirationDateUtc, string scope, string redirectUri)
    {
        if (string.IsNullOrEmpty(accessToken))
            throw new ArgumentNullException(nameof(accessToken));

        if (string.IsNullOrEmpty(refreshToken))
            throw new ArgumentNullException(nameof(refreshToken));

        AccessToken = accessToken;
        RefreshToken = refreshToken;
        ExpirationDateUtc = expirationDateUtc;
        Scope = scope;
        RedirectUri = redirectUri;

        if (IsExpired)
            throw new ArgumentException($"Token is already expired. Expiration UTC: {expirationDateUtc}", nameof(expirationDateUtc));
    }
}

public interface ITokenProvider
{
    Task<GoogleToken?> FindTokenAsync();

    Task<GoogleToken> GetTokenAsync();

    void SetToken(GoogleToken token);
}

public class InMemoryTokenProvider : ITokenProvider
{
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly Action<string> _log;
    private GoogleToken? _token;

    public async Task<GoogleToken?> FindTokenAsync()
    {
        if (_token == null)
            return null;

        if (_token.IsExpired)
        {
            _token = await GoogleApiTokenClient.ByRefreshToken(_token, _clientId, _clientSecret);
            _log($"New google token. Expiration UTC: {_token.ExpirationDateUtc}");
        }

        return _token;
    }

    public async Task<GoogleToken> GetTokenAsync()
    {
        return await FindTokenAsync() ?? throw new Exception("Token is not specified");
    }

    public void SetToken(GoogleToken token)
    {
        _token = token;
    }

    public InMemoryTokenProvider(string clientId, string clientSecret, Action<string> log)
    {
        _clientId = clientId;
        _clientSecret = clientSecret;
        _log = log;
    }
}
using System.Text.Json.Serialization;

namespace GFunc.Photos;

public class GoogleToken
{
    public string AccessToken { get; }
    public string RefreshToken { get; }
    public DateTime ExpirationDateUtc { get; }

    public bool IsExpired => DateTime.UtcNow > ExpirationDateUtc;

    public GoogleToken(string accessToken, string refreshToken, DateTime expirationDateUtc)
    {
        if (string.IsNullOrEmpty(accessToken))
            throw new ArgumentNullException(nameof(accessToken));

        if (string.IsNullOrEmpty(refreshToken))
            throw new ArgumentNullException(nameof(refreshToken));

        AccessToken = accessToken;
        RefreshToken = refreshToken;
        ExpirationDateUtc = expirationDateUtc;

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
    private readonly ILogger _logger;
    private GoogleToken? _token;
    private readonly string _tokenFile;

    public async Task<GoogleToken?> FindTokenAsync()
    {
        if (_token == null)
            return null;

        if (_token.IsExpired)
        {
            _token = await GoogleApiTokenClient.ByRefreshToken(_token.RefreshToken, _clientId, _clientSecret);
            _logger.Info($"New google token. Expiration UTC: {_token.ExpirationDateUtc}");
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

        using Stream str = File.Create(_tokenFile);
        using StreamWriter writer = new StreamWriter(str);

        writer.Write(token.RefreshToken);
    }

    public InMemoryTokenProvider(string clientId, string clientSecret, string configFolder, ILogger logger)
    {
        _clientId = clientId;
        _clientSecret = clientSecret;
        _logger = logger;

        _tokenFile = Path.Combine(configFolder, "token");
        
        if (File.Exists(_tokenFile))
        {
            var token = GoogleApiTokenClient.ByRefreshToken(File.ReadAllText(_tokenFile), clientId, clientSecret).Result;
            logger.Info($"Found refresh token in '{_tokenFile}'. Expiration UTC: {token.ExpirationDateUtc}");
            _token = token;
        }
    }
}
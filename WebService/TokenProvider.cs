namespace WebService;

public class GoogleToken
{
    public string AccessToken { get; }
    public string RefreshToken { get; }

    public GoogleToken(string accessToken, string refreshToken)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
    }
}

public interface ITokenProvider
{
    GoogleToken? FindToken();

    void SetToken(GoogleToken token);
}

public class InMemoryTokenProvider : ITokenProvider
{
    private GoogleToken? _token;

    public GoogleToken? FindToken()
    {
        return _token;
    }

    public void SetToken(GoogleToken token)
    {
        _token = token;
    }
}
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc;

namespace WebService.Controllers;

public class AuthController : Controller
{
    private const string Scope = "https://www.googleapis.com/auth/photoslibrary.readonly"; //use %20 for concatinate scopes
    private const string UrlStr = "https://accounts.google.com/o/oauth2/v2/auth?response_type=code&client_id={0}&scope={1}&redirect_uri={2}";

    private readonly string _redirectUri;
    private readonly ITokenProvider _tokenProvider;
    private readonly string _clientId;
    private readonly string _clientSecret;

    [HttpGet("auth")]
    public IActionResult Auth()
    {
        return Redirect(string.Format(UrlStr, _clientId, Scope, Uri.EscapeDataString(_redirectUri)));
    }
    
    [HttpGet("response")]
    public async Task<IActionResult> AuthCodeResponse()
    {
        string[]? queryParameters = Request.QueryString.Value?.Split(new[] {'&', '?'},
            StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        if (queryParameters == null)
            return BadRequest();
        
        string code = queryParameters.First(x => x.StartsWith("code")).Split('=')[1];

        using HttpClient client = new HttpClient();
        
        string tokenRequestURI = "https://www.googleapis.com/oauth2/v4/token";
        string tokenRequestBody = $"code={code}&redirect_uri={Uri.EscapeDataString(_redirectUri)}&client_id={_clientId}&client_secret={_clientSecret}&scope={Scope}&grant_type=authorization_code";

        var body = new StringContent(tokenRequestBody);
        body.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

        using var request = new HttpRequestMessage(HttpMethod.Post, tokenRequestURI);
        request.Headers.Accept.ParseAdd("text/html");
        request.Headers.Accept.ParseAdd("application/xhtml+xml");
        request.Headers.Accept.ParseAdd("application/xml");
        
        using var response = await client.PostAsync(tokenRequestURI, body);

        string responseText = await response.Content.ReadAsStringAsync();

        string accessToken = FindValueByKey(responseText, "access_token");
        string refreshToken = FindValueByKey(responseText, "refresh_token");
        
        _tokenProvider.SetToken(new GoogleToken(accessToken, refreshToken));

        return Ok("Account has been added");
    }

    public AuthController(IConfiguration config, ITokenProvider tokenProvider, IServer server)
    {
        _tokenProvider = tokenProvider;
        _clientId = config["ClientId"];
        _clientSecret = config["ClientSecret"];

        string? url = server.Features.Get<IServerAddressesFeature>()
            ?.Addresses.FirstOrDefault(x => x.StartsWith("http:", StringComparison.OrdinalIgnoreCase));

        if (string.IsNullOrEmpty(url))
            throw new Exception("Cannot find server URL");

        _redirectUri = $"{url}/response";

        if (string.IsNullOrEmpty(_clientId))
            throw new Exception("ClientId must be specified");

        if (string.IsNullOrEmpty(_clientSecret))
            throw new Exception("ClientSecret must be specified");
    }
    
    private static string FindValueByKey(string text, string key)
    {
        try
        {
            int index = text.IndexOf(key);
            string value = text[index..].Split('"')[2];
            return value;
        }
        catch
        {
            return string.Empty;
        }
    }
}
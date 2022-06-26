using System.Net.Http.Headers;
using GFunc.Photos;
using Microsoft.AspNetCore.Mvc;

namespace WebService.Controllers;

public class AuthController : Controller
{
    private const string Scope = "https://www.googleapis.com/auth/photoslibrary.readonly";
    private const string UrlStr = "https://accounts.google.com/o/oauth2/v2/auth?response_type=code&client_id={0}&scope={1}&redirect_uri={2}&access_type=offline";

    private readonly ITokenProvider _tokenProvider;
    private readonly ILogger<AuthController> _logger;
    private readonly string _clientId;
    private readonly string _clientSecret;

    [HttpGet("auth")]
    public IActionResult Auth()
    {
        return Redirect(string.Format(UrlStr, _clientId, Scope, Uri.EscapeDataString($"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/response")));
    }
    
    [HttpGet("response")]
    public async Task<IActionResult> AuthCodeResponse()
    {
        string[]? queryParameters = Request.QueryString.Value?.Split(new[] {'&', '?'},
            StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        if (queryParameters == null)
            return BadRequest();
        
        string code = queryParameters.First(x => x.StartsWith("code")).Split('=')[1];
        string redirectUri = Uri.EscapeDataString($"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/response");
        
        var token = await GoogleApiTokenClient.ByCodeAsync(code, redirectUri, _clientId, _clientSecret, Scope);
        
        _logger.LogInformation($"New google token. Expiration UTC: {token.ExpirationDateUtc}");

        _tokenProvider.SetToken(token);

        return Ok("Account has been added");
    }

    public AuthController(IConfiguration config, ITokenProvider tokenProvider, ILogger<AuthController> logger)
    {
        _tokenProvider = tokenProvider;
        _logger = logger;
        _clientId = ConfigHelper.GetConfigValue("ClientId", config);
        _clientSecret = ConfigHelper.GetConfigValue("ClientSecret", config);
    }
}
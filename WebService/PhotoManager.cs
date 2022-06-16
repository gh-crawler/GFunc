using GFunc.Photos;
using GFunc.Photos.Model;

namespace WebService;

public class PhotoManager : BackgroundService
{
    private readonly ITokenProvider _tokenProvider;
    private MediaRule? _rule;
    private readonly string _albumId;
    private readonly string _basePath;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var token = await AwaitTokenAsync(stoppingToken);
        _rule = new MediaRule(new[] {new AlbumCondition(_albumId)}, new[] {new DateTimeCondition(DateTime.UtcNow)}, new GooglePhotosProvider(token.AccessToken, token.RefreshToken), new SaveToLocalAction(_basePath));

        await Task.Run(() => Loop(stoppingToken), stoppingToken);
    }

    public PhotoManager(ITokenProvider tokenProvider, IConfiguration config)
    {
        _tokenProvider = tokenProvider;
        _albumId = config["AlbumId"] ?? throw new Exception("Album ID must be specified");
        _basePath = config["BasePath"] ?? throw new Exception("BasePath must be specified");
    }

    private async Task<GoogleToken> AwaitTokenAsync(CancellationToken cancellationToken)
    {
        GoogleToken? token = null;
        var timeout = TimeSpan.FromSeconds(10);
        
        while (token == null)
        {
            await Task.Delay(timeout, cancellationToken);
            token = _tokenProvider.FindToken();
        }

        return token;
    }

    private async Task Loop(CancellationToken token)
    {
        TimeSpan timeout = TimeSpan.FromMinutes(5);

        while (!token.IsCancellationRequested)
        {
            await _rule!.InvokeAsync();
            await Task.Delay(timeout, token);
        }
    }
}
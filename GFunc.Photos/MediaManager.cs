namespace GFunc.Photos;

public class MediaManager
{
    private readonly string _albumId;
    private readonly IMediaProvider _provider;
    private readonly IMediaAction _action;
    private readonly HashSet<string> _handledItems = new(StringComparer.OrdinalIgnoreCase);
    private readonly DateTime _startTimeUtc;

    public async Task InvokeAsync()
    {
        foreach (var item in await _provider.GetMediaAsync(_albumId))
        {
            if (item.Metadata.CreationTimeUtc < _startTimeUtc || _handledItems.Contains(item.Id))
                continue;
            
            await _action.InvokeAsync(item);
            _handledItems.Add(item.Id);
        }
    }
    
    public MediaManager(string albumId, IMediaProvider provider, IMediaAction action)
    {
        _albumId = albumId;
        _provider = provider;
        _action = action;
        _startTimeUtc = DateTime.UtcNow;
    }
}
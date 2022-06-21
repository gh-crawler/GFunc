using System.Diagnostics;
using GFunc.Photos.Model;

namespace GFunc.Photos;

public class MediaRule
{
    private readonly IReadOnlyCollection<IPreCondition> _preConditions;
    private readonly IReadOnlyCollection<IPostCondition> _postConditions;
    private readonly IMediaProvider _provider;
    private readonly IReadOnlyCollection<IMediaAction> _actions;
    private readonly Action<string>? _log;
    private readonly HashSet<string> _handledItems = new(StringComparer.OrdinalIgnoreCase);
    
    public string Name { get; }

    public async Task<int> InvokeAsync()
    {
        int counter = 0;

        var stopwatch = Stopwatch.StartNew();
        
        var mediaItems = await _provider.GetMediaAsync(_preConditions);
        
        stopwatch.Stop();

        _log?.Invoke($"{mediaItems.Count} media items have been received in {stopwatch.Elapsed}");

        foreach (var item in mediaItems)
        {
            if (_handledItems.Contains(item.Id) || !MeetsPostConditions(item))
                continue;

            foreach (var action in _actions)
            {
                await action.InvokeAsync(item);
            }

            counter++;
            _handledItems.Add(item.Id);
        }

        return counter;
    }

    public MediaRule(IReadOnlyCollection<IPreCondition> preConditions, IReadOnlyCollection<IPostCondition> postConditions, IMediaProvider provider, IReadOnlyCollection<IMediaAction> actions, string name, Action<string>? log = null)
    {
        _preConditions = preConditions;
        _postConditions = postConditions;
        _provider = provider;
        _actions = actions;
        _log = log;
        Name = name;
    }

    private bool MeetsPostConditions(MediaItem item)
    {
        if (_postConditions.Count == 0)
            return true;

        return _postConditions.All(x => x.Meets(item));
    }
}
using GFunc.Photos.Model;

namespace GFunc.Photos;

public class MediaRule
{
    private readonly IReadOnlyCollection<IPreCondition> _preConditions;
    private readonly IReadOnlyCollection<IPostCondition> _postConditions;
    private readonly IMediaProvider _provider;
    private readonly IMediaAction _action;
    private readonly HashSet<string> _handledItems = new(StringComparer.OrdinalIgnoreCase);

    public async Task InvokeAsync()
    {
        foreach (var item in await _provider.GetMediaAsync(_preConditions))
        {
            if (_handledItems.Contains(item.Id) || !MeetsPostConditions(item))
                continue;
            
            await _action.InvokeAsync(item);
            _handledItems.Add(item.Id);
        }
    }

    public MediaRule(IReadOnlyCollection<IPreCondition> preConditions, IReadOnlyCollection<IPostCondition> postConditions, IMediaProvider provider, IMediaAction action)
    {
        _preConditions = preConditions;
        _postConditions = postConditions;
        _provider = provider;
        _action = action;
    }

    private bool MeetsPostConditions(MediaItem item)
    {
        if (_postConditions.Count == 0)
            return true;

        return _postConditions.All(x => x.Meets(item));
    }
}
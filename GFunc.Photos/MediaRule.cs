using GFunc.Photos.Model;

namespace GFunc.Photos;

public class MediaRule
{
    private readonly IReadOnlyCollection<IPreCondition> _preConditions;
    private readonly IReadOnlyCollection<IPostCondition> _postConditions;
    private readonly IMediaProvider _provider;
    private readonly IReadOnlyCollection<IMediaAction> _actions;
    private readonly HashSet<string> _handledItems = new(StringComparer.OrdinalIgnoreCase);
    
    public string Name { get; }

    public async Task<int> InvokeAsync()
    {
        int counter = 0;
        
        foreach (var item in await _provider.GetMediaAsync(_preConditions))
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

    public MediaRule(IReadOnlyCollection<IPreCondition> preConditions, IReadOnlyCollection<IPostCondition> postConditions, IMediaProvider provider, IReadOnlyCollection<IMediaAction> actions, string name)
    {
        _preConditions = preConditions;
        _postConditions = postConditions;
        _provider = provider;
        _actions = actions;
        Name = name;
    }

    private bool MeetsPostConditions(MediaItem item)
    {
        if (_postConditions.Count == 0)
            return true;

        return _postConditions.All(x => x.Meets(item));
    }
}
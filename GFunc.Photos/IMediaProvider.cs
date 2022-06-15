using GFunc.Photos.Model;

namespace GFunc.Photos;

public interface IMediaProvider
{
    Task<IReadOnlyCollection<MediaItem>> GetMediaAsync(IReadOnlyCollection<IPreCondition> preConditions);
}
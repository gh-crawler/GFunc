using GFunc.Photos.Model;

namespace GFunc.Photos;

public interface IMediaAction
{
    Task InvokeAsync(MediaItem item);
}
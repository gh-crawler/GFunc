using GFunc.Photos.Model;

namespace GFunc.Photos;

public interface IMediaAction
{
    Task InvokeAsync(MediaItem item);
}

public class SaveToLocalAction : IMediaAction
{
    private readonly string _basePath;

    public async Task InvokeAsync(MediaItem item)
    {
        string path = Path.Combine(_basePath, item.BuildPath());

        using var client = new HttpClient();

        string tail = item.IsVideo() ? "dv" : "w10000-h10000-d";
        string url = $"{item.Url}={tail}";
        
        await using Stream str = await (await client.GetAsync(url)).Content.ReadAsStreamAsync();
        await using Stream fileStr = File.Create(path);
        
        await str.CopyToAsync(fileStr);
    }

    public SaveToLocalAction(string basePath)
    {
        _basePath = basePath;
    }
}
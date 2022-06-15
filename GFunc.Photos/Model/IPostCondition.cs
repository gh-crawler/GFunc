namespace GFunc.Photos.Model;

public interface IPostCondition
{
    bool Meets(MediaItem item);
}

public class DateTimeCondition : IPostCondition
{
    private readonly DateTime _conditionTimeUtc;
    
    public bool Meets(MediaItem item) => item.Metadata.CreationTimeUtc > _conditionTimeUtc;

    public DateTimeCondition() : this(DateTime.UtcNow)
    {
    }

    public DateTimeCondition(DateTime conditionTimeUtc)
    {
        _conditionTimeUtc = conditionTimeUtc;
    }
}
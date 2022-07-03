
namespace WebService;

public class LoggerAdapter : GFunc.Photos.ILogger
{
    private readonly ILogger _loggerImplementation;

    public void Info(string message)
    {
        _loggerImplementation.LogInformation(message);
    }

    public void Warning(string message)
    {
        _loggerImplementation.LogWarning(message);
    }

    public void Error(string message)
    {
        _loggerImplementation.LogError(message);
    }

    public LoggerAdapter(ILogger loggerImplementation)
    {
        _loggerImplementation = loggerImplementation;
    }
}
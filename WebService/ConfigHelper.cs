static class ConfigHelper
{
    public static string GetConfigValue(string name, IConfiguration configuration)
    {
        string value = configuration[name];

        if (string.IsNullOrEmpty(value))
            throw new Exception($"Value '{name}' was not found in config");

        return value;
    }
}
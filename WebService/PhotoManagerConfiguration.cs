namespace WebService;

public class PhotoManagerConfiguration
{
    public Dictionary<string, RuleConfig> Rules { get; set; }
}

public class RuleConfig
{
    public Dictionary<string, string> Conditions { get; set; }
    public Dictionary<string, string> Actions { get; set; }
}
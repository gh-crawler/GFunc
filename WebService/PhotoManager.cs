using GFunc.Photos;
using GFunc.Photos.Model;

namespace WebService;

public class PhotoManager : BackgroundService
{
    private readonly ITokenProvider _tokenProvider;
    private readonly ILogger<PhotoManager> _logger;
    private readonly PhotoManagerConfiguration _configuration;
    private List<MediaRule> _rules;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await AwaitTokenAsync(stoppingToken);

        _rules = new List<MediaRule>(_configuration.Rules.Count);

        foreach ((string name, RuleConfig rule) in _configuration.Rules)
        {
            var (preConditions, postConditions) = BuildConditions(rule);
            var actions = BuildActions(rule);
            
            _rules.Add(new MediaRule(preConditions, postConditions, new GooglePhotosProvider(_tokenProvider), actions, name, log: msg => _logger.LogInformation(msg)));
            
            _logger.LogInformation($"Rule '{name}': {preConditions.Count} pre-conditions, {postConditions.Count} post-conditions, {actions.Count} actions");
        }

        await Task.Run(() => Loop(stoppingToken), stoppingToken);
    }

    private static (List<IPreCondition> preConditions, List<IPostCondition> postConditions) BuildConditions(RuleConfig ruleConfig)
    {
        var preConditions = new List<IPreCondition>();
        var postConditions = new List<IPostCondition>
        {
            new DateTimeCondition()
        };

        foreach (var condition in ruleConfig.Conditions)
        {
            switch (condition.Key)
            {
                case "albumId":
                    preConditions.Add(new AlbumCondition(condition.Value));
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(condition), condition.Key, string.Empty);
            }
        }

        return (preConditions, postConditions);
    }
    
    private static List<IMediaAction> BuildActions(RuleConfig ruleConfig)
    {
        List<IMediaAction> actions = new(ruleConfig.Actions.Count);

        foreach (var configAction in ruleConfig.Actions)
        {
            switch (configAction.Key)
            {
                case "toLocal":
                    actions.Add(new SaveToLocalAction(configAction.Value));
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(configAction), configAction.Key, string.Empty);
            }
        }

        return actions;
    }

    public PhotoManager(ITokenProvider tokenProvider, IConfiguration config, ILogger<PhotoManager> logger)
    {
        _tokenProvider = tokenProvider;
        _logger = logger;
        _configuration = config.Get<PhotoManagerConfiguration>();

        if (_configuration.Rules.Count == 0)
            throw new Exception("Rule config is empty");
    }

    private async Task AwaitTokenAsync(CancellationToken cancellationToken)
    {
        GoogleToken? token = null;
        var timeout = TimeSpan.FromSeconds(10);
        
        while (token == null)
        {
            await Task.Delay(timeout, cancellationToken);
            token = await _tokenProvider.FindTokenAsync();
        }
    }

    private async Task Loop(CancellationToken token)
    {
        TimeSpan timeout = TimeSpan.FromMinutes(1);

        while (!token.IsCancellationRequested)
        {
            foreach (var rule in _rules)
            {
                int count = await rule.InvokeAsync();
                
                if (count > 0)
                    _logger.LogInformation($"Rule '{rule.Name}': {count} new items handled");
            }

            await Task.Delay(timeout, token);
        }
    }
}
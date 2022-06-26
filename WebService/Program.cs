using GFunc.Photos;
using WebService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddHostedService<PhotoManager>();

string clientId = ConfigHelper.GetConfigValue("ClientId", builder.Configuration);
string clientSecret = ConfigHelper.GetConfigValue("ClientSecret", builder.Configuration);
string configFolder = Path.GetDirectoryName(ConfigHelper.GetConfigValue("GFUNC_CONFIG", builder.Configuration)) ?? throw new Exception("Cannot find config folder");

builder.Services.AddSingleton<ITokenProvider>(provider =>
{
    var logger = provider.GetService<ILogger<InMemoryTokenProvider>>();
    return new InMemoryTokenProvider(clientId, clientSecret, configFolder, s => logger.LogInformation(s));
});

builder.Logging.ClearProviders().AddConsole();

string configPath = builder.Configuration.GetValue<string>("GFUNC_CONFIG");

if (string.IsNullOrEmpty(configPath))
    throw new Exception("Cannot find config file");

builder.Configuration.AddJsonFile(configPath, false);

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
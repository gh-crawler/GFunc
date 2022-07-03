using GFunc.Photos;
using WebService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddHostedService<PhotoManager>();

string clientId = ConfigHelper.GetConfigValue("ClientId", builder.Configuration);
string clientSecret = ConfigHelper.GetConfigValue("ClientSecret", builder.Configuration);
string configFolder = builder.Environment.IsDevelopment() ? ConfigHelper.GetConfigValue("GFUNC_CONFIG", builder.Configuration) : "/config";

builder.Services.AddSingleton<ITokenProvider>(provider =>
{
    var logger = provider.GetService<ILogger<InMemoryTokenProvider>>();
    return new InMemoryTokenProvider(clientId, clientSecret, configFolder, s => logger.LogInformation(s));
});

builder.Logging.ClearProviders().AddConsole();

string configPath = Path.Combine(configFolder, "config.json");

builder.Configuration.AddJsonFile(configPath, true);

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
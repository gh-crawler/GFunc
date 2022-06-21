using WebService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddHostedService<PhotoManager>();
builder.Services.AddSingleton<ITokenProvider, InMemoryTokenProvider>();

builder.Logging.ClearProviders().AddConsole();

string configPath = builder.Configuration.GetValue<string>("GFUNC_CONFIG");

if (string.IsNullOrEmpty(configPath))
    throw new Exception("Cannot find config file");

builder.Configuration.AddJsonFile(configPath, false);

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

app.Run();
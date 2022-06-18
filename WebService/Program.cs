using WebService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddHostedService<PhotoManager>();
builder.Services.AddSingleton<ITokenProvider, InMemoryTokenProvider>();

builder.Logging.ClearProviders().AddConsole();

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

app.Run();
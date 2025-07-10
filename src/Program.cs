
using Yarp.ReverseProxy;

var builder = WebApplication.CreateBuilder(args);

// Add YARP reverse proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// Optionally, keep a root endpoint
app.MapGet("/", () => "OllamaYarpProject Reverse Proxy is running.");

// Map the proxy endpoints
app.MapReverseProxy();

app.Run();

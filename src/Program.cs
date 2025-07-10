using YARP.ReverseProxy.Forwarder;
using YARP.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

// ...existing code...

var app = builder.Build();

// Add this middleware before YARP proxy
app.Use(async (context, next) =>
{
    if (context.Request.Path.Equals("/api/tags", StringComparison.OrdinalIgnoreCase))
    {
        context.Request.Path = "/models";
    }
    await next();
});

app.MapReverseProxy();
// ...existing code...
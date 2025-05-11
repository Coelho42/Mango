using Mango.GatewaySolution.Extensions;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot(builder.Configuration);
builder.AddAppAuthentication();
var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.UseOcelot().GetAwaiter().GetResult();
app.UseAuthentication();

app.Run();

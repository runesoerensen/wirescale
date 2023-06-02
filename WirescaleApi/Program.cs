using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Net.Http.Headers;
using WirescaleApi;

var builder = WebApplication.CreateBuilder(args);

// Add a simple console logger for logging to stdout.
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole();

builder.Services.AddControllers();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.Authority = $"https://{builder.Configuration["Auth0:Domain"]}/";
    options.Audience = builder.Configuration["Auth0:Audience"];
});

var dockerSecretsDirectory = "/run/secrets/";
builder.Configuration.AddKeyPerFile(dockerSecretsDirectory);

builder.Services.AddHttpClient<IWgrestApiClient, WgrestApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["WgrestApi:BaseAddress"]);
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", builder.Configuration["wgrest_auth_token"]);
});

builder.Services.AddTransient<IpAddressAllocator>();
builder.Services.AddTransient<WireguardManager>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

using Microsoft.Extensions.Configuration;

namespace WirescaleCli;

public class Program
{
    public static async Task Main(string[] args)
    {
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false);

        var configuration = configurationBuilder.Build();

        var accessTokenProvider = new AcessTokenProvider(configuration["Auth0:Domain"], configuration["Auth0:ClientId"], configuration["Auth0:RedirectUri"]);
        var accessToken = await accessTokenProvider.GetAccessToken();
    }
}

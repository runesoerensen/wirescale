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

        var wireguardKeyPairGenerator = new WireguardKeyPairGenerator();
        var wireguardKeyPair = wireguardKeyPairGenerator.Generate();

        var wirescaleApiClient = new WirescaleApiClient(configuration["WirescaleApiBaseUri"]);
        try
        {
            var wireguardPeerRegistration = await wirescaleApiClient.RegisterPublicKey(accessToken, wireguardKeyPair);

            var wireguardConfigurationWriter = new WireguardConfigurationWriter("wg-wirescale.conf");
            wireguardConfigurationWriter.Write(wireguardKeyPair, wireguardPeerRegistration);

            Console.WriteLine("You are now logged in.");
        }
        catch (HttpRequestException exception)
        {
            Console.WriteLine($"Failed to call the Wirescale API. Status code: {exception.StatusCode}");
        }
    }
}

using System.Net;
using System.Net.Sockets;
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

        var wirescaleApiBaseUri = configuration["WirescaleApiBaseUri"];
        string accessToken;
        try
        {
            accessToken = await accessTokenProvider.GetAccessToken(wirescaleApiBaseUri);
        }
        catch (AuthorizationRequestException exception)
        {
            Console.Error.WriteLine(exception.Message);

            Environment.Exit(1);
            return;
        }

        var wireguardKeyPairGenerator = new WireguardKeyPairGenerator();
        var wireguardKeyPair = wireguardKeyPairGenerator.Generate();

        var wirescaleApiClient = new WirescaleApiClient(wirescaleApiBaseUri);
        try
        {
            var wireguardPeerRegistration = await wirescaleApiClient.RegisterPublicKey(accessToken, wireguardKeyPair);

            var wireguardConfigurationWriter = new WireguardConfigurationWriter("wg-wirescale.conf");
            wireguardConfigurationWriter.Write(wireguardKeyPair, wireguardPeerRegistration);

            Console.WriteLine("You are now logged in.");
        }
        catch (HttpRequestException exception)
        {
            if (exception.InnerException is SocketException)
            {
                Console.Error.WriteLine($"Could not connect to the Wirescale API server. Error: {exception.Message}");
            }
            else if (exception.StatusCode == HttpStatusCode.Unauthorized)
            {
                Console.Error.WriteLine("Your WireGuard public key could not be registered with the Wirescale API due to an authorization issue. Please try logging in again.");
            }
            else if (exception.StatusCode == HttpStatusCode.BadRequest)
            {
                Console.Error.WriteLine($"The Wirescale API returned an HTTP 400 with the message: {exception.Message}");
            }
            else if (exception.StatusCode == HttpStatusCode.InternalServerError)
            {
                Console.Error.WriteLine("The Wirescale API was unable to process your request due to an internal server error. The error has been logged and you may file a bug report if this issue is not resolved shortly");
            }
            else
            {
                throw;
            }
        }
        catch (WireguardCommandException exception)
        {
            Console.Error.WriteLine(exception.Message);
        }
    }
}

using System.Diagnostics;
using System.Text;

namespace WirescaleCli;

public class WireguardConfigurationWriter
{
    private readonly string _configurationFilePath;

    public WireguardConfigurationWriter(string configurationFileName)
    {
        //Should move this to a different directory (e.g. the distro-specific wg folder, `~/.wirescale` or similar.
        _configurationFilePath = Path.Combine(Directory.GetCurrentDirectory(), configurationFileName);
    }

    public void Write(WireguardKeyPair wireguardKeyPair, WirescalePeerRegistration peerRegistration)
    {
        StringBuilder configurationBuilder = new StringBuilder();

        configurationBuilder.AppendLine("[Interface]");
        configurationBuilder.AppendLine($"PrivateKey = {wireguardKeyPair.PrivateKey}");
        configurationBuilder.AppendLine($"Address = {peerRegistration.AllowedIps.Single()}");

        configurationBuilder.AppendLine();

        configurationBuilder.AppendLine("[Peer]");
        configurationBuilder.AppendLine($"PublicKey = {peerRegistration.ServerPublicKey}");
        configurationBuilder.AppendLine($"AllowedIPs = {peerRegistration.ServerAllowedIps.Single()}");
        configurationBuilder.AppendLine($"Endpoint = {peerRegistration.ServerEndpoint}");

        File.WriteAllText(_configurationFilePath, configurationBuilder.ToString());

        //Move wireguard logic/configuration elsewhere
        var process = Process.Start(new ProcessStartInfo
        {
            FileName = "wg-quick",
            Arguments = $"up {_configurationFilePath}",
            UseShellExecute = false,
            CreateNoWindow = true,
        });

        process.WaitForExit();
        if (process.ExitCode != 0)
        {
            // The command `wirescale logout` is not yet implemented, so provide an alternative solution for early adopters of Wirescale
            throw new WireguardCommandException($"Failed to configure Wireguard interface. Try logging out (`wirescale logout`), or manually remove the interface using `wg-quick down {_configurationFilePath}`");
        }
    }
}

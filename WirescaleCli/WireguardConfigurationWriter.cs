using System.Diagnostics;
using System.Text;

namespace WirescaleCli;

public class WireguardConfigurationWriter
{
    private readonly string _configurationFilePath;

    public WireguardConfigurationWriter(string configurationFilePath)
    {
        _configurationFilePath = configurationFilePath;
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
            Arguments = $"up ./{_configurationFilePath}",
            UseShellExecute = false,
            CreateNoWindow = true,
        });

        process.WaitForExit();
        if (process.ExitCode != 0)
        {
            throw new Exception("Failed to configure WireGuard.");
        }
    }
}

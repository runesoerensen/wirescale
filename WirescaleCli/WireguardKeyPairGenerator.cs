using System.Diagnostics;
namespace WirescaleCli;

public class WireguardKeyPairGenerator
{
    public WireguardKeyPair Generate()
    {
        // Generate a new private key
        var privateKeyProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "wg",
                Arguments = "genkey",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        privateKeyProcess.Start();
        var privateKey = privateKeyProcess.StandardOutput.ReadToEnd().Trim();
        privateKeyProcess.WaitForExit();

        // Generate the corresponding public key
        var publicKeyProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "wg",
                Arguments = "pubkey",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        publicKeyProcess.Start();
        publicKeyProcess.StandardInput.WriteLine(privateKey);
        publicKeyProcess.StandardInput.Flush();
        publicKeyProcess.StandardInput.Close();

        var publicKey = publicKeyProcess.StandardOutput.ReadToEnd().Trim();

        publicKeyProcess.WaitForExit();

        return new WireguardKeyPair
        {
            PublicKey = publicKey,
            PrivateKey = privateKey,
        };
    }
}

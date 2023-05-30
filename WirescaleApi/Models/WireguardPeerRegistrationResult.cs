namespace WirescaleApi.Models;

public class WireguardPeerRegistrationResult
{
    public string PublicKey { get; set; }
    public string UrlSafePublicKey { get; set; }
    public List<string> AllowedIps { get; set; }

    public string ServerPublicKey { get; set; }
    public string ServerEndpoint { get; set; }
    public List<string> ServerAllowedIps { get; set; }
}

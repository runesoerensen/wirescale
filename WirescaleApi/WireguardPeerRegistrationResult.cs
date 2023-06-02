namespace WirescaleApi;

public class WireguardPeerRegistrationResult
{
    public string PublicKey { get; set; }               // This is the same public key the user provided for registration.
    public string UrlSafePublicKey { get; set; }        // Client could use this with other RESTful endpoints, e.g. to GET/DELETE the peer registration.
    public List<string> AllowedIps { get; set; }        // Client will use this to set their assigned virtual network address(es).

    public string ServerPublicKey { get; set; }         // Client will use this to register the server peer locally.
    public string ServerEndpoint { get; set; }          // The server's public IP address and port.
    public List<string> ServerAllowedIps { get; set; }  // IP ranges that the server can route traffic to on the virtual network.
}

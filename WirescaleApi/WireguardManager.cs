using NetTools;

namespace WirescaleApi;

public class WireguardManager
{
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    private readonly IWgrestApiClient _wgrestApiClient;
    private readonly IpAddressAllocator _ipAddressAllocator;

    public WireguardManager(IWgrestApiClient wgrestApiClient, IpAddressAllocator ipAddressAllocator)
    {
        _wgrestApiClient = wgrestApiClient;
        _ipAddressAllocator = ipAddressAllocator;
    }

    public async Task<WireguardPeerRegistrationResult> RegisterNewPeer(string clientPublicKey)
    {
        var deviceName = "wg-wirescale";
        var wireguardDevices = await _wgrestApiClient.GetWgrestDevices();
        var wirescaleDevice = wireguardDevices.Single(x => x.Name == deviceName);

        try
        {
            // Make sure multiple instances of WireGuardManager synchronizes access to peer registration
            // to avoid allocating the same allowed-ips to more than one peer.
            await _semaphore.WaitAsync();

            return await RegisterDevicePeer(wirescaleDevice, clientPublicKey);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<WireguardPeerRegistrationResult> RegisterDevicePeer(WgrestDevice wirescaleDevice, string clientPublicKey)
    {
        var wireguardPeers = await _wgrestApiClient.GetWgrestPeers(wirescaleDevice.Name);
        if (wireguardPeers.Any(x => x.PublicKey == clientPublicKey))
        {
            throw new InvalidOperationException("Public key is already registered as a peer");
        }

        // We currently only support one (IPv4) network. Throw an exception if device has more or less than that.
        var wireguardNetworkCidr = wirescaleDevice.Networks.Single();

        // Get the currently registered peer IP ranges
        var currentPeerIpRanges = wireguardPeers.Select(x => IPAddressRange.Parse(x.AllowedIps.Single()));

        var clientPeerIpAddress = _ipAddressAllocator.AllocateIpAddress(wireguardNetworkCidr, currentPeerIpRanges);

        var wgrestPeer = await _wgrestApiClient.CreateWgrestPeer(wirescaleDevice.Name, new WgrestPeer
        {
            AllowedIps = new List<string> { $"{clientPeerIpAddress}/32" },
            PublicKey = clientPublicKey,
        });

        var wireguardInterfaceOptions = await _wgrestApiClient.GetWgrestDeviceOptions(wirescaleDevice.Name);

        return new WireguardPeerRegistrationResult
        {
            UrlSafePublicKey = wgrestPeer.UrlSafePublicKey,
            PublicKey = wgrestPeer.PublicKey,
            AllowedIps = wgrestPeer.AllowedIps,

            ServerPublicKey = wirescaleDevice.PublicKey,
            ServerEndpoint = $"{wireguardInterfaceOptions.Host}:{wirescaleDevice.ListenPort}",
            ServerAllowedIps = new List<string> { wireguardNetworkCidr },
        };
    }
}

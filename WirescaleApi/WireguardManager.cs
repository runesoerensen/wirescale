using WirescaleApi.Models;

namespace WirescaleApi;

public class WireguardManager
{
    private readonly IWgrestApiClient _wgrestApiClient;
    private readonly IpAddressAllocator _ipAddressAllocator;

    public WireguardManager(IWgrestApiClient wgrestApiClient, IpAddressAllocator ipAddressAllocator)
    {
        _wgrestApiClient = wgrestApiClient;
        _ipAddressAllocator = ipAddressAllocator;
    }

    public async Task<WireguardPeerRegistrationResult> RegisterNewPeer(string clientPublicKey)
    {
        throw new NotImplementedException();
    }
}

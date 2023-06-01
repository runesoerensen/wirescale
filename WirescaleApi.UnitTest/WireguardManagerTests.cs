using System.Net;
using Moq;
using NetTools;
using Xunit;

namespace WirescaleApi.UnitTest;

public class WireguardManagerTests
{
    private readonly Mock<IWgrestApiClient> _mockWgrestApiClient;
    private readonly Mock<IpAddressAllocator> _mockIpAddressAllocator;
    private readonly WireguardManager _wireguardManager;

    private readonly string _testClientPublicKey = "clientPublicKey";
    private readonly string _testClientUrlSafePublicKey = "clientUrlSafePublicKey";

    private readonly string _testServerPeerPublicKey = "testServerPeerPublicKey";
    private readonly string _testWireguardDeviceName = "wg-wirescale";
    private readonly string _testWireguardNetworkCidr = "10.8.0.0/24";

    private readonly string _testServerPublicIpAddress = "198.51.100.1";
    private readonly int _testServerPublicListenPort = 12345;

    private readonly IPAddress _testAllocatedClientWireguardIpAddress = IPAddress.Parse("10.8.0.2");

    public WireguardManagerTests()
    {
        _mockWgrestApiClient = new Mock<IWgrestApiClient>();
        _mockIpAddressAllocator = new Mock<IpAddressAllocator>();
        _wireguardManager = new WireguardManager(_mockWgrestApiClient.Object, _mockIpAddressAllocator.Object);

        SetupMocks();
    }

    private void SetupMocks()
    {
        var wgrestDevices = new List<WgrestDevice>
        {
            new WgrestDevice
            {
                Name = _testWireguardDeviceName,
                PublicKey = _testServerPeerPublicKey,
                Networks = new List<string> { _testWireguardNetworkCidr },
                ListenPort = _testServerPublicListenPort,
            }
        };

        var wgrestPeers = new List<WgrestPeer>
        {
            new WgrestPeer { PublicKey = "anotherClientPublicKey" }
        };

        var wgrestDeviceOptions = new WgrestDeviceOptions
        {
            Host = _testServerPublicIpAddress,
        };

        _mockWgrestApiClient
            .Setup(x => x.GetWgrestDevices())
            .ReturnsAsync(wgrestDevices);

        _mockWgrestApiClient
            .Setup(x => x.GetWgrestPeers(It.IsAny<string>()))
            .ReturnsAsync(wgrestPeers);

        _mockIpAddressAllocator
            .Setup(x => x.AllocateIpAddress(It.IsAny<string>(), It.IsAny<IEnumerable<IPAddressRange>>()))
            .Returns(_testAllocatedClientWireguardIpAddress);

        _mockWgrestApiClient
            .Setup(x => x.CreateWgrestPeer(It.IsAny<string>(), It.IsAny<WgrestPeer>()))
            .ReturnsAsync((string deviceName, WgrestPeer peer) =>
            {
                peer.UrlSafePublicKey = _testClientUrlSafePublicKey;
                return peer;
            });

        _mockWgrestApiClient
            .Setup(x => x.GetWgrestDeviceOptions(It.IsAny<string>()))
            .ReturnsAsync(wgrestDeviceOptions);
    }

    [Fact]
    public async Task RegisterNewPeer_ShouldReturn_PeerRegistrationWithAllocatedIpAndServerPeerInformation()
    {
        var result = await _wireguardManager.RegisterNewPeer(_testClientPublicKey);

        Assert.Equal(_testClientPublicKey, result.PublicKey);
        Assert.Equal($"{_testAllocatedClientWireguardIpAddress}/32", result.AllowedIps.Single());
        Assert.Equal(_testClientUrlSafePublicKey, result.UrlSafePublicKey);

        Assert.Equal(_testServerPeerPublicKey, result.ServerPublicKey);
        Assert.Equal(_testWireguardNetworkCidr, result.ServerAllowedIps.Single());
        Assert.Equal($"{_testServerPublicIpAddress}:{_testServerPublicListenPort}", result.ServerEndpoint);
    }

    [Fact]
    public async Task RegisterNewPeer_ShouldThrowException_WhenPublicKeyAlreadyRegistered()
    {
        var wgrestPeers = new List<WgrestPeer>
        {
            new WgrestPeer { PublicKey = _testClientPublicKey }
        };

        _mockWgrestApiClient
            .Setup(x => x.GetWgrestPeers(It.IsAny<string>()))
            .ReturnsAsync(wgrestPeers);

        await Assert.ThrowsAsync<PeerAlreadyRegisteredException>(() => _wireguardManager.RegisterNewPeer(_testClientPublicKey));
    }
}

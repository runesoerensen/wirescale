using System.Net;
using NetTools;
using Xunit;

namespace WirescaleApi.UnitTest;

public class IpAddressAllocatorTests
{
    private IpAddressAllocator _ipAddressAllocator;
    private IPAddress _serverIpAddress;
    private IEnumerable<IPAddressRange> _peerIpRanges;

    public IpAddressAllocatorTests()
    {
        _ipAddressAllocator = new IpAddressAllocator();
        _serverIpAddress = IPAddress.Parse("10.0.0.1");
        _peerIpRanges = new List<IPAddressRange>
        {
            new IPAddressRange(IPAddress.Parse("10.0.0.2"), IPAddress.Parse("10.0.0.10")),
        };
    }

    [Theory]
    [InlineData("10.0.0.1/24", "10.0.0.11")]
    public void AllocateIpAddress_ReturnsNextAvailableIpAddress(string networkCidr, string expectedIp)
    {
        IPAddress result = _ipAddressAllocator.AllocateIpAddress(networkCidr, _peerIpRanges);

        Assert.Equal(IPAddress.Parse(expectedIp), result);
    }
}

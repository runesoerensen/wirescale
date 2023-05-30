using System.Net;
using NetTools;

namespace WirescaleApi;

public class IpAddressAllocator
{
    public virtual IPAddress AllocateIpAddress(string networkCidr, IEnumerable<IPAddressRange> peerIpRanges)
    {
        var ipNetwork = IPAddressRange.Parse(networkCidr);
        var serverIpAddress = IPAddress.Parse(networkCidr.Split("/")[0]);

        foreach (var ipAddress in ipNetwork)
        {
            if (ipAddress.Equals(serverIpAddress)
                || peerIpRanges.Any(x => x.Contains(ipAddress))
                || ipAddress.ToString().EndsWith(".0"))
            {
                continue;
            }

            return ipAddress;
        }
        throw new NotImplementedException();
    }
}

using System.Net;
using NetTools;

namespace WirescaleApi;

public class IpAddressAllocator
{
    public virtual IPAddress AllocateIpAddress(string networkCidr, IEnumerable<IPAddressRange> peerIpRanges)
    {
        throw new NotImplementedException();
    }
}

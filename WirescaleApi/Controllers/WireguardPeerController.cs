using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WirescaleApi.Models;

namespace WirescaleApi.Controllers;

[ApiController]
[Route("[controller]")]
public class WireguardPeerController : ControllerBase
{
    [HttpPost]
    public WireguardPeer Create([FromBody] string publicKey)
    {
        throw new NotImplementedException();
    }
}

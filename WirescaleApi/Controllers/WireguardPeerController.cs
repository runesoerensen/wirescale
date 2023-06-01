using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WirescaleApi.Controllers;

[ApiController]
[Route("[controller]")]
public class WireguardPeerController : ControllerBase
{
    private readonly WireguardManager _wireguardManager;

    public WireguardPeerController(WireguardManager wireguardManager)
    {
        _wireguardManager = wireguardManager;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<WireguardPeerRegistrationResult>> Create([FromBody] string publicKey)
    {
        try
        {
            var registrationResult = await _wireguardManager.RegisterNewPeer(publicKey);

            return Ok(registrationResult);
        }
        // This is currently the only type of exception that can be caused by a client's input return HTTP 400 with error message on that.
        catch (PeerAlreadyRegisteredException)
        {
            return BadRequest("Public key is already registered as a peer");
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WirescaleApi.Models;

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
        catch (InvalidOperationException exception)
        {
            return BadRequest(exception.Message);
        }
    }
}

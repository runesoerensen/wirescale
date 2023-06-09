﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WirescaleApi.Controllers;

[ApiController]
[Route("[controller]")]
public class WireguardPeerController : ControllerBase
{
    private readonly ILogger<WireguardPeerController> _logger;
    private readonly WireguardManager _wireguardManager;

    public WireguardPeerController(ILogger<WireguardPeerController> logger, WireguardManager wireguardManager)
    {
        _logger = logger;
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
        // Log all other exceptions and return HTTP 500
        catch (Exception exception)
        {
            _logger.LogError(exception, exception.Message);

            return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
        }
    }
}

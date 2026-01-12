using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PingController(IPingService pingService) : ControllerBase
{
    private readonly IPingService _pingService = pingService;

    [HttpGet]
    public async Task<ActionResult<Ping?>> GetPingAsync()
    {
        var ping = await _pingService.PingAsync();
        if (ping == null)
        {
            return NotFound();

        }
        return Ok(ping);
    }
}
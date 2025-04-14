using Microsoft.AspNetCore.Mvc;
using WebApi.Data;
using WebApi.Models;

namespace WebApi.Controllers;

[ApiController]
[Route("api/queue")]
public class QueueController(ApplicationDbContext context) : ControllerBase
{
    
    [HttpGet("window/{windowId}")]
    public async Task<IActionResult> GetWindowQueue(int windowId)
    {
        // TODO: return clients pending/called/being served, linked to the given windowId
        
        return NoContent(); // TODO: delete this placeholder
    }
    
    [HttpGet("called")]
    public async Task<IActionResult> GetCalledClients()
    {
        // TODO: return client numbers that are being called (function for system, one number for each window)
        
        return NoContent(); // TODO: delete this placeholder
    }
    
    [HttpGet("awaiting")]
    public async Task<IActionResult> GetAwaitingClients()
    {
        // TODO: return client numbers that are awaiting (function for system, max first 20-25 numbers)
        
        return NoContent(); // TODO: delete this placeholder
    }
}
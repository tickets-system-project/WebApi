using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAwaitingClients()
    {
        var awaitingClients = await context.Queue
            .Include(q => q.Reservation)
            .ThenInclude(r => r.Status)
            .Where(q => q.Reservation.Status.Name == "Oczekujący") 
            .OrderBy(q => q.Reservation.Time) 
            .Take(20) 
            .Select(q => q.QueueCode) 
            .ToListAsync();

        return Ok(awaitingClients);
    }
}
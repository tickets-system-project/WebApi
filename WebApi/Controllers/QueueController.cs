using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Models;
using WebApi.Models.DTOs.Queue;

namespace WebApi.Controllers;

[ApiController]
[Route("api/queue")]
public class QueueController(ApplicationDbContext context) : ControllerBase
{
    // Dodać metode do wyświetlania wszystkich okeinek dla admina, a getWindowQueue to ednpoint do przycisku dla kazdego 
    // okienka aby wyświetlić wszytstkich petentów, na początku max 2 osoby
    [HttpGet("window/{windowId}")]
    public async Task<IActionResult> GetWindowQueue(int windowId)
    {
        // TODO: return clients pending/called/being served, linked to the given windowId
        
        return NoContent(); // TODO: delete this placeholder
    }
    [HttpGet("called")]
    [ProducesResponseType(typeof(IEnumerable<CalledClientDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCalledClients()
    {
        var calledClients = await context.Queue
           
            .Where(q => q.QueueCode != null && q.WindowID != null)
        
       
            .Include(q => q.Window)
            .Include(q => q.Reservation)
            .ThenInclude(r => r.Status)
        
            
            .Where(q => q.Reservation != null && 
                        q.Reservation.Status != null &&
                        q.Reservation.Status.Name == "Wezwany")
        
            
            .Select(q => new CalledClientDto
            {
                QueueCode = q.QueueCode!,
                WindowNumber = q.Window!.WindowNumber
            })
            .ToListAsync();

        return Ok(calledClients);
    }


    
    [HttpGet("awaiting")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAwaitingClients()
    {
        var awaitingClients = await context.Queue
            .Include(q => q.Reservation)
            .ThenInclude(r => r.Status)
            .Where(q => q.Reservation.Status.Name == "Oczekujący" && q.QueueCode != null) 
            .OrderBy(q => q.Reservation.Time) 
            .Take(20) 
            .Select(q => q.QueueCode) 
            .ToListAsync();

        return Ok(awaitingClients);
    }
}
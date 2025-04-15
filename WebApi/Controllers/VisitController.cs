using Microsoft.AspNetCore.Mvc;
using WebApi.Data;
using WebApi.Models;

namespace WebApi.Controllers;

[ApiController]
[Route("api/visit")]
public class VisitController(ApplicationDbContext context) : ControllerBase
{
    [HttpPost("schedule")]
    public async Task<IActionResult> ScheduleVisit([FromBody] VisitRequest visitRequest)
    {
        // TODO: Validate required fields (FirstName, LastName, Pesel, Email)
        // TODO: Validate PESEL (11 digits etc.)
        // TODO: Validate email (email format)
        // TODO: Optionally validate phone number (if provided)
        // TODO: If all data is correct, send confirmation email
        // TODO: Return a JSON response with visit details:
        // - Visit date
        // - Visit time
        // - Visit title
        // - Confirmation code
        // - Email address where confirmation was sent
        return NoContent(); // TODO: delete this placeholder
    }
    
    [HttpGet("details/{visitId}")]
    public async Task<IActionResult> GetVisitDetails(int visitId)
    {
        // TODO: Retrieve the details of the visit for the given ID

        // TODO: Return the visit details as JSON
        return NoContent(); // TODO: delete this placeholder
    }

    [HttpPost("validateCode")]
    public async Task<IActionResult> ValidateCode([FromBody] VisitRequest visitRequest)
    {
        
        // TODO: get code, check if it is valid (right code FOR SPECIFIED DAY - no tomorrow or X days later/earlier)
        
        return NoContent(); // TODO: delete this placeholder
    }

    [HttpPost("unregistered/schedule")]
    public async Task<IActionResult> ScheduleUnregistered([FromBody] VisitRequest visitRequest)
    {
        // TODO: get visit purpose and date with time, check currentReservation <= maxReservation
        // TODO: return code number or error (frontend should check this but better be safe than sorry)
        
        return NoContent(); // TODO: delete this placeholder
    }

    [HttpGet("currentReservations")]
    public async Task<IActionResult> GetCurrentReservations([FromQuery] DateTime dateTime)
    {
        // TODO: return currentReservation number for specified date and time
        
        return NoContent(); // TODO: delete this placeholder
    }
}

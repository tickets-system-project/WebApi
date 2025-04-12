using Microsoft.AspNetCore.Mvc;
using WebApi.Models;

namespace WebApi.Controllers;

[ApiController]
[Route("api/visit")]
public class VisitController : ControllerBase
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
    
    [HttpGet("details/{clientId}")]
    public async Task<IActionResult> GetVisitDetails(int clientId)
    {
        // TODO: Retrieve the details of the visit for the given client ID

        // TODO: Return the visit details as JSON
        return NoContent(); // TODO: delete this placeholder
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Models.Entities;

namespace WebApi.Controllers;

[ApiController]
[Route("api/clerk/{id}")]
public class ClerkController(ApplicationDbContext context) : ControllerBase
{
    [HttpPost("nextCase")]
    public async Task<IActionResult> CallNextClient()
    {
        // TODO: Retrieve the first client from the queue (status "waiting")
    
        // TODO: Change the client's status to "called"

        // TODO: Return response indicating success or failure
        return NoContent(); // TODO: delete this placeholder
    }
    
    [HttpPost("finishCase/{caseId}")]
    public async Task<IActionResult> FinishCase(int caseId)
    {
        // TODO: Change the case's status to "completed"

        // TODO: Return response indicating success or failure
        return NoContent(); // TODO: delete this placeholder
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Models.Entities;

namespace WebApi.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(ApplicationDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        return await context.Users.ToListAsync();
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        var user = await context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        return user;
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] User user)
    {
        // TODO: Add logic to create a user (e.g., validation, saving to DB)
        return NoContent(); // TODO: delete this placeholder
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] User user)
    {
        // TODO: Check if the user exists by id
        // TODO: Validate and update the user data in the database
        // TODO: Return appropriate status (Ok, NotFound, BadRequest)
        return NoContent(); // TODO: delete this placeholder
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        // TODO: Check if the user exists by id
        // TODO: If user exists, delete from the database
        // TODO: Return appropriate status (Ok, NotFound)
        return NoContent(); // TODO: delete this placeholder
    }
    
    [HttpGet("search")]
    public async Task<IActionResult> SearchUsers(string firstName = null, string lastName = null, string position = null, string caseCategory = null)
    {
        // TODO: Search users by first name using LIKE or regex
        // TODO: Search users by last name using LIKE or regex
        // TODO: Filter users by position (if provided)
        // TODO: Filter users by case category (if provided)
            
        return NoContent(); // TODO: delete this placeholder
    }
}

using Microsoft.AspNetCore.Mvc;
using WebApi.Models;

namespace WebApi.Controllers;

// TODO
[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        // check login request
        
        // ok
        // return Ok(new { Message = "Log in successful" });
        
        // error
        // return Unauthorized(new { Message = "Incorrect login or password" });
        return NoContent(); // TODO: delete this placeholder
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
    {
        // check register request

        // ok
        // return Ok(new { Message = "Registration successful" });

        // error
        // return BadRequest(new { Message = "User already exists" });
        return NoContent(); // TODO: delete this placeholder
    }
}

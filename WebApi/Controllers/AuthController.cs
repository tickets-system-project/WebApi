using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebApi.Data;
using WebApi.Models.DTOs;
using WebApi.Models.Entities;

namespace WebApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(ApplicationDbContext context, IConfiguration configuration) : ControllerBase
{
    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.ID.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.ID.ToString()),
            new Claim(ClaimTypes.Role, user.Role.Name)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException()));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private bool VerifyPasswordHash(string password, string storedHash)
    {
        // var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        return BCrypt.Net.BCrypt.Verify(password, storedHash);
    }
    
    [HttpPost("login")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [Consumes("application/json")]
    [Produces("application/json")]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        
        var user = await context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Username == loginRequest.Username);
        
        if (user == null) return Unauthorized("Invalid username or password.");
        
        var loginData = await context.LoginData.FirstOrDefaultAsync(ld => ld.UserID == user.ID);
        if (loginData == null || !VerifyPasswordHash(loginRequest.Password, loginData.PasswordHash))
            return Unauthorized("Invalid username or password.");
        
        var token = GenerateJwtToken(user);
        return Ok(new { Token = token });
    }
    
    // [HttpPost("register")]
    // public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
    // {
    //     // check register request
    //
    //     // ok
    //     // return Ok(new { Message = "Registration successful" });
    //
    //     // error
    //     // return BadRequest(new { Message = "User already exists" });
    //     return NoContent(); // TODO: delete this placeholder
    // }
}

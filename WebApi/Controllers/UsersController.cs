using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Models.DTOs.User;
using WebApi.Models.Entities;

namespace WebApi.Controllers;

[ApiController]
[Route("api/user")]
public class UsersController(ApplicationDbContext context, IMapper mapper) : ControllerBase
{
    private async Task<IActionResult?> ValidateEmailAsync(User user, User? existingUser)
    {
        if (!string.IsNullOrWhiteSpace(user.Email) && !new EmailAddressAttribute().IsValid(user.Email))
        {
            return BadRequest("Invalid email format.");
        }

        if (existingUser == null || string.Equals(existingUser.Email, user.Email, StringComparison.OrdinalIgnoreCase)) return null;
        
        var emailExists = await context.Users.AnyAsync(u => u.Email == user.Email);
        
        return emailExists ? BadRequest("Email is already in use.") : null;
    }

    private IActionResult? CheckPrivilegesToUpdate(User existingUser)
    {
        var loggedInUsername = User.Identity?.Name;
        var isAdmin = User.IsInRole("Administrator");

        // Not admin or user updating their own account
        if (!isAdmin && !string.Equals(existingUser.Username, loggedInUsername, StringComparison.OrdinalIgnoreCase))
        {
            return Forbid(); // 403
        }

        return null;
    }

    private async Task<bool> UsernameExistsAsync(string username)
    {
        return await context.Users.AnyAsync(u => u.Username == username);
    }
    
    [HttpGet]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [Produces("application/json")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        var users = await context.Users.Include(u => u.Role).ToListAsync();
        var userDtos = mapper.Map<IEnumerable<UserDto>>(users);
        return Ok(userDtos);
    }
    
    [HttpGet("{id}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(UserDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [Produces("application/json")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        var user = await context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.ID == id);
        if (user == null) return NotFound();

        var userDto = mapper.Map<UserDto>(user);
        return Ok(userDto);
    }
    
    [HttpPost]
    [Authorize(Roles = "Administrator")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(UserDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [Produces("application/json")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto userDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var user = mapper.Map<User>(userDto);
        
        var role = await context.Roles.FirstOrDefaultAsync(r => r.Name == userDto.RoleName);
        if (role == null) return BadRequest($"Role '{userDto.RoleName}' does not exist.");
        
        user.RoleID = role.ID;
        
        if (string.IsNullOrWhiteSpace(user.FirstName) || string.IsNullOrWhiteSpace(user.LastName) ||
            string.IsNullOrWhiteSpace(user.Username))
        {
            return BadRequest("FirstName, LastName and Username cannot be empty.");
        }
        
        var emailValidation = await ValidateEmailAsync(user, null);
        if (emailValidation != null) return emailValidation;
        
        if (await UsernameExistsAsync(user.Username)) return BadRequest("Username is already taken.");

        context.Users.Add(user);
        await context.SaveChangesAsync();
        
        await context.Entry(user).Reference(u => u.Role).LoadAsync();
        var createdDto = mapper.Map<UserDto>(user);
        return CreatedAtAction(nameof(GetUser), new { id = user.ID }, createdDto);
    }

    [HttpPut("{id}")]
    [Authorize]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(UserDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [Produces("application/json")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto userDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        
        var existingUser = await context.Users.FindAsync(id);
        if (existingUser == null) return NotFound();
        
        var privilegeCheck = CheckPrivilegesToUpdate(existingUser);
        if (privilegeCheck != null) return privilegeCheck;
        
        var updatedUser = mapper.Map(userDto, existingUser);
        
        var role = await context.Roles.FirstOrDefaultAsync(r => r.Name == userDto.RoleName);
        if (role == null) return BadRequest($"Role '{userDto.RoleName}' does not exist.");
        
        if (existingUser.RoleID != role.ID) updatedUser.RoleID = role.ID;
        
        var emailValidation = await ValidateEmailAsync(updatedUser, existingUser);
        if (emailValidation != null) return emailValidation;
        
        if (!string.IsNullOrWhiteSpace(userDto.Username) &&
            !string.Equals(userDto.Username, existingUser.Username, StringComparison.OrdinalIgnoreCase))
        {
            if (await UsernameExistsAsync(userDto.Username))
                return BadRequest("Username is already taken.");
        }

        await context.SaveChangesAsync();
        
        await context.Entry(updatedUser).Reference(u => u.Role).LoadAsync();
        var resultDto = mapper.Map<UserDto>(updatedUser);
        return Ok(resultDto);
    }
    
    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [Produces("application/json")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await context.Users.FindAsync(id);
        if (user == null) return NotFound();

        context.Users.Remove(user);
        await context.SaveChangesAsync();
        
        return NoContent();
    }
    
    [HttpGet("search")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [Produces("application/json")]
    public async Task<IActionResult> SearchUsers(string? firstName = null, string? lastName = null, string? position = null) //string? caseCategory = null)
    {
        var query = context.Users.Include(u => u.Role).AsQueryable();

        if (!string.IsNullOrWhiteSpace(firstName))
            query = query.Where(u => u.FirstName.ToLower().Contains(firstName.ToLower()));

        if (!string.IsNullOrWhiteSpace(lastName))
            query = query.Where(u => u.LastName.ToLower().Contains(lastName.ToLower()));

        if (!string.IsNullOrWhiteSpace(position))
            query = query.Where(u => u.Role.Name.ToLower().Contains(position.ToLower()));

        var result = await query.ToListAsync();
        var resultDtos = mapper.Map<List<UserDto>>(result);

        return Ok(resultDtos);
    }
}

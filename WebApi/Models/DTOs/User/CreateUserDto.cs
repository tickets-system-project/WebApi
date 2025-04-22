using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.DTOs.User;

public class CreateUserDto
{
    [Required]
    public required string FirstName { get; set; }
    
    [Required]
    public required string LastName { get; set; }
    
    [Required]
    public required string Username { get; set; }
    
    [Required]
    public required string Email { get; set; }
    
    [Required]
    public required int RoleID { get; set; }
}
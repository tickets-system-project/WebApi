using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.DTOs.User;

public class UserDto
{
    public int ID { get; set; }
    
    [Required]
    public required string FirstName { get; set; }
    
    [Required]
    public required string LastName { get; set; }
    
    [Required]
    public required string Username { get; set; }
    
    [Required]
    public required string Email { get; set; }
    
    [Required]
    public required string RoleName { get; set; }
}
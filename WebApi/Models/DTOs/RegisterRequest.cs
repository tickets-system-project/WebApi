using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.DTOs;

public class RegisterRequest
{
    [Required]
    public required string Username { get; set; }
    
    [Required]
    public required string Password { get; set; }
    
    [Required]
    public required string Email { get; set; }
}
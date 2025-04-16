namespace WebApi.Models.DTOs.User;

public class UserDto
{
    public int ID { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Username { get; set; }
    public string? Email { get; set; }
    public string? RoleName { get; set; }
}
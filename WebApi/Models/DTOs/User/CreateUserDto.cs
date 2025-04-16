namespace WebApi.Models.DTOs.User;

public class CreateUserDto
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Username { get; set; }
    public string? Email { get; set; }
    public int? RoleID { get; set; }
}
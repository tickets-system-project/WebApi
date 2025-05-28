namespace WebApi.Models.DTOs.User;

public class UpdateUserDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public int? RoleID { get; set; }
    public string? Password { get; set; }
}
using System.ComponentModel.DataAnnotations;

namespace WebApp.Models;

public class User
{
    public int ID { get; set; }
    [MaxLength(50)]
    public string FirstName { get; set; }
    [MaxLength(50)]
    public string LastName { get; set; }
    [MaxLength(50)]
    public string Username { get; set; }
    [MaxLength(100)]
    public string Email { get; set; }
    public int RoleID { get; set; }
}

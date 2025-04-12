using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApi.Models.Entities;

public class User
{
    [Key]
    public int ID { get; set; }

    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; }

    [Required]
    [MaxLength(50)]
    public string LastName { get; set; }

    [Required]
    [MaxLength(50)]
    public string Username { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }

    [ForeignKey("Role")]
    public int? RoleID { get; set; }
    public Role? Role { get; set; }
}

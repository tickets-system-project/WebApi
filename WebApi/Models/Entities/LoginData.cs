using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApi.Models.Entities;

public class LoginData
{
    [Key]
    public int ID { get; set; }
    
    [ForeignKey("User")]
    public int? UserID { get; set; }
    public User? User { get; set; }

    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; }
}
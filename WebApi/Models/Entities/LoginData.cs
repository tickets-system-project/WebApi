using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.Entities;

public class LoginData
{
    public int ID { get; set; }
    public int UserID  { get; set; }
    [MaxLength(255)]
    public string PasswordHash { get; set; }
}
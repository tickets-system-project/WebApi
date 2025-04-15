using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.Entities;

public class Client
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
    [StringLength(11)] // fixed length
    public string PESEL { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }
}
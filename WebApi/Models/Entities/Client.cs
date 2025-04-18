using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.Entities;

public class Client
{
    [Key]
    public int ID { get; set; }

    [Required]
    [MaxLength(50)]
    public required string FirstName { get; set; }

    [Required]
    [MaxLength(50)]
    public required string LastName { get; set; }

    [Required]
    [StringLength(11)] // fixed length
    public required string PESEL { get; set; }

    [Required]
    [MaxLength(100)]
    [EmailAddress]
    public required string Email { get; set; }
    
    [MaxLength(20)]
    [Phone]
    public string? Phone { get; set; }
}
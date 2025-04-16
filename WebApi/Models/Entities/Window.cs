using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.Entities;

public class Window
{
    [Key]
    public int ID { get; set; }

    [Required]
    [MaxLength(50)]
    public required string WindowNumber { get; set; }
}
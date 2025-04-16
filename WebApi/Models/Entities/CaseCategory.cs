using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.Entities;

public class CaseCategory
{
    [Key]
    public int ID { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Letter { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }
}

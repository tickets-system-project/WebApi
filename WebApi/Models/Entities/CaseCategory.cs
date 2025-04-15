using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.Entities;

public class CaseCategory
{
    [Key]
    public int ID { get; set; }

    [Required]
    [MaxLength(100)]
    public string Letter { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.DTOs.Visit;

public class SlotDto
{
    [Required]
    public DateOnly Date { get; set; }
    
    [Required]
    public TimeOnly Time { get; set; }
}
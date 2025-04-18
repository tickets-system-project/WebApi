using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.DTOs.Visit;

public class VisitResponse
{
    [Required]
    public required DateOnly Date { get; set; }
    
    [Required]
    public required TimeOnly Time { get; set; }
    
    [Required]
    public required string CaseCategoryName { get; set; }
    
    [Required]
    public required string ConfirmationCode { get; set; }
    
    [Required]
    public required string Email { get; set; }
}
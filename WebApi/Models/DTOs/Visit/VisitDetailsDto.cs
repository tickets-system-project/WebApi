using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.DTOs.Visit;

public class VisitDetailsDto
{
    [Required]
    public required string FirstName { get; set; }
    
    [Required]
    public required string LastName { get; set; }
    
    [Required]
    public required string PESEL { get; set; }
    
    [Required]
    public required string Email { get; set; }
    
    public string? Phone { get; set; }
    
    [Required]
    public required int CaseCategoryID { get; set; }
    
    [Required]
    public required DateOnly Date { get; set; }
    
    [Required]
    public required TimeOnly Time { get; set; }
    
    [Required]
    public required int StatusID { get; set; }
}
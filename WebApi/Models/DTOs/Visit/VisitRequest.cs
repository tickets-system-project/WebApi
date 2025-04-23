using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.DTOs.Visit;

public class VisitRequest
{
    [Required]
    public required string FirstName { get; set; }

    [Required]
    public required string LastName { get; set; }

    [Required]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "PESEL must be 11 characters.")]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "PESEL must consist of 11 digits.")]
    public required string Pesel { get; set; }

    [Required]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public required string Email { get; set; }
    
    [Phone(ErrorMessage = "Invalid phone number.")]
    public string? Phone { get; set; }

    [Required]
    public required int CaseCategoryID { get; set; }

    [Required]
    public required DateOnly Date { get; set; }

    [Required]
    public required TimeOnly Time { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.DTOs;

public class VisitRequest
{
    [Required]
    public required string FirstName { get; set; }

    [Required]
    public required string LastName { get; set; }

    [Required]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "PESEL must be 11 characters.")]
    public required string Pesel { get; set; }

    [Required]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public required string Email { get; set; }
    
    [Phone(ErrorMessage = "Invalid phone number.")]
    public string? Phone { get; set; }
}

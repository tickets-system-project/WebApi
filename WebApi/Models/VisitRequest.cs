using System.ComponentModel.DataAnnotations;

namespace WebApi.Models;

public class VisitRequest
{
    [Required]
    public string FirstName { get; set; }

    [Required]
    public string LastName { get; set; }

    [Required]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "PESEL must be 11 characters.")]
    public string Pesel { get; set; }

    [Required]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string Email { get; set; }
    
    [Phone(ErrorMessage = "Invalid phone number.")]
    public string Phone { get; set; }
}

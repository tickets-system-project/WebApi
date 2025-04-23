using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.DTOs.Visit;

public class UnregisteredVisitRequest
{
    [Required]
    public required int CaseCategoryID { get; set; }
}
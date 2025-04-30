using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.DTOs.Category;

public class CreateCaseCategoryDto
{
    [Required] public required string Letter { get; set; }

    [Required] public required string Name { get; set; }
}
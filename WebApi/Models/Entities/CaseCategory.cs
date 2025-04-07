using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.Entities;

public class CaseCategory
{
    public int ID { get; set; }
    [MaxLength(100)]
    public string Name { get; set; }
}

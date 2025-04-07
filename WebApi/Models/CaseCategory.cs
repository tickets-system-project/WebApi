using System.ComponentModel.DataAnnotations;

namespace WebApp.Models;

public class CaseCategory
{
    public int ID { get; set; }
    [MaxLength(100)]
    public string Name { get; set; }
}

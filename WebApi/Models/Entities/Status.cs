using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.Entities;

public class Status
{
    public int ID { get; set; }
    
    [MaxLength(50)]
    public string Name { get; set; }
}
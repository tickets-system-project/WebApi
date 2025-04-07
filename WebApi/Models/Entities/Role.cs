using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.Entities;

public class Role
{
    public int ID { get; set; }
    [MaxLength(50)]
    public string Name { get; set; }
}
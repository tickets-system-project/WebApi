using System.ComponentModel.DataAnnotations;

namespace WebApp.Models;

public class Role
{
    public int ID { get; set; }
    [MaxLength(50)]
    public string Name { get; set; }
}
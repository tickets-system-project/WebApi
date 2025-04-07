using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.Entities;

public class Window
{
    public int ID { get; set; }
    [MaxLength(50)]
    public string WindowNumber { get; set; }
}
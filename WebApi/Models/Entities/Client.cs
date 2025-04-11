using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.Entities;

public class Client
{
    public int ID { get; set; }
    
    [MaxLength(50)]
    public string FirstName { get; set; }
    
    [MaxLength(50)]
    public string LastName { get; set; }
    
    [MaxLength(11)]
    public string PESEL { get; set; }
    
    [MaxLength(20)]
    public string? Phone { get; set; }
    
    [MaxLength(100)]
    public string? Email { get; set; }
}
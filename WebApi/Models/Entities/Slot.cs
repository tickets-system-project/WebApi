using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApi.Models.Entities;

public class Slot
{
    [Key]
    public int ID { get; set; }

    [ForeignKey("Category")]
    public int? CategoryID { get; set; }
    public CaseCategory? Category { get; set; }

    public DateOnly? Date { get; set; }
    
    public TimeOnly? Time { get; set; }

    public int MaxReservations { get; set; }
    
    public int CurrentReservations { get; set; }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApi.Models.Entities;

public class Queue
{
    [Key]
    public int ID { get; set; }

    [ForeignKey("Window")]
    public int? WindowID { get; set; }
    public Window? Window { get; set; }

    [ForeignKey("Reservation")]
    public int? ReservationID { get; set; }
    public Reservation? Reservation { get; set; }
    
    [MaxLength(50)]
    public string? QueueCode {get; set;}
}
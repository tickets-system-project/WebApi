using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.Entities;

public class Reservation
{
    public int ID { get; set; }
    public int ClientID { get; set; }
    public int CategoryID { get; set; }
    public DateTime Date { get; set; }
    public DateTime Time { get; set; }
    [MaxLength(50)]
    public string ConfirmationCode { get; set; }
}
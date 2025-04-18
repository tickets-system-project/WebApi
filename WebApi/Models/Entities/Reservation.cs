using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApi.Models.Entities;

public class Reservation
{
    [Key]
    public int ID { get; set; }

    [ForeignKey("Client")]
    public int? ClientID { get; set; }
    public Client? Client { get; set; }

    [ForeignKey("Category")]
    public int? CategoryID { get; set; }
    public CaseCategory? Category { get; set; }

    [ForeignKey("Status")]
    public int? StatusID { get; set; }
    public Status? Status { get; set; }

    public DateOnly? Date { get; set; }
    
    public TimeOnly? Time { get; set; }

    [MaxLength(50)]
    public string? ConfirmationCode { get; set; }
}
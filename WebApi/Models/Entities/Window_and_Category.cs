using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApi.Models.Entities;

public class Window_and_Category
{
    [Key]
    public int ID { get; set; }

    [ForeignKey("Window")]
    public int? WindowID { get; set; }
    public Window? Window { get; set; }

    [ForeignKey("Category")]
    public int? CategoryID { get; set; }
    public CaseCategory? Category { get; set; }

    public DateTime? Date { get; set; }

    [ForeignKey("Clerk")]
    public int? ClerkID { get; set; }
    public User? Clerk { get; set; }
}
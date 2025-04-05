namespace DefaultNamespace;

public class Reservation
{
    public int ID { get; set; }
    public int ClientID { get; set; }
    public int CategoryID { get; set; }
    public DateTime Date { get; set; }
    public DateTime Time { get; set; }
    public string ConfirmationCode { get; set; }
}
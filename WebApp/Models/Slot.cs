namespace DefaultNamespace;

public class Slot
{
    public int ID { get; set; }
    public int CategoryID { get; set; }
    public DateTime Date { get; set; }
    public DateTime Time { get; set; }
    public int MaxReservations { get; set; }
    public int CurrentReservations { get; set; }
}
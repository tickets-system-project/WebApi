namespace WebApi.Models.DTOs.Queue;

public class ClientDto
{
    public string QueueCode { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public int Category { get; set; }
    public int Status { get; set; }
}
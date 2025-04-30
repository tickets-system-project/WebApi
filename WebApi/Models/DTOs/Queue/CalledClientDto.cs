namespace WebApi.Models.DTOs.Queue;

public class CalledClientDto
{
    public required string QueueCode { get; set; }
    public required string WindowNumber { get; set; }
}
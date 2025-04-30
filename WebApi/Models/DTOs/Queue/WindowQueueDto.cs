namespace WebApi.Models.DTOs.Queue;

public class WindowQueueDto
{
    public string WindowNumber { get; set; } = default!;
    public List<ClientDto> Clients { get; set; } = new();
}
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Models.DTOs.Queue;

namespace WebApi.Controllers;

[ApiController]
[Route("api/queue")]
public class QueueController(ApplicationDbContext context) : ControllerBase
{
    [HttpGet("window/{windowId}")]
    [Authorize(Roles = "Administrator,Urzędnik")]
    [ProducesResponseType(typeof(List<ClientDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetWindowQueue(int windowId)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdString, out var clerkId))
        {
            return BadRequest("Invalid user ID in token.");
        }

        var user = await context.Users.FindAsync(clerkId);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        var today = DateOnly.FromDateTime(DateTime.Now);

        var categoryInfo = await context.Windows_and_Categories
            .Where(wc => wc.WindowID == windowId &&
                         wc.ClerkID == clerkId &&
                         wc.Date == today)
            .Select(wc => new
            {
                wc.CategoryID
            })
            .FirstOrDefaultAsync();

        if (categoryInfo == null)
        {
            return StatusCode(403, "Brak uprawnień do tego okienka lub kategorii");
        }

        var waitingClients = await context.Reservations
            .Include(r => r.Client)
            .Where(r => r.CategoryID == categoryInfo.CategoryID &&
                        r.Status.Name == "Oczekujący")
            .OrderBy(r => r.Time)
            .Select(r => new ClientDto
            {
                QueueCode = context.Queue
                    .Where(q => q.ReservationID == r.ID)
                    .Select(q => q.QueueCode)
                    .FirstOrDefault() ?? "BRAK_KODU",

                FullName = $"{r.Client.FirstName} {r.Client.LastName}",
                Category = r.Category.ID,
                Status = r.Status.ID
            })
            .ToListAsync();

        return Ok(waitingClients);
    }


    [HttpGet("called")]
    [Authorize(Roles = "Administrator, Urzędnik")]
    [ProducesResponseType(typeof(IEnumerable<CalledClientDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCalledClients()
    {
        var calledClients = await context.Queue
            .Where(q => q.QueueCode != null && q.WindowID != null)
            .Include(q => q.Window)
            .Include(q => q.Reservation)
            .ThenInclude(r => r.Status)
            .Where(q => q.Reservation != null &&
                        q.Reservation.Status != null &&
                        q.Reservation.Status.Name == "Wezwany")
            .Select(q => new CalledClientDto
            {
                QueueCode = q.QueueCode!,
                WindowNumber = q.Window!.WindowNumber
            })
            .ToListAsync();

        return Ok(calledClients);
    }


    [HttpGet("awaiting")]
    [Authorize(Roles = "Administrator, Urzędnik")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAwaitingClients()
    {
        var awaitingClients = await context.Queue
            .Include(q => q.Reservation)
            .ThenInclude(r => r.Status)
            .Where(q => q.Reservation.Status.Name == "Oczekujący" && q.QueueCode != null)
            .OrderBy(q => q.Reservation.Time)
            .Take(20)
            .Select(q => q.QueueCode)
            .ToListAsync();

        return Ok(awaitingClients);
    }

    [HttpGet("window-clients")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(IEnumerable<WindowQueueDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAllClientsPerWindow()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var statusNames = new[] { "Oczekujący", "Wezwany", "Obsługiwany" };

        var windowCategories = await context.Windows_and_Categories
            .Where(wc => wc.Date == today)
            .Include(wc => wc.Window)
            .ToListAsync();

        var reservations = await context.Reservations
            .Include(r => r.Client)
            .Include(r => r.Status)
            .Include(r => r.Category)
            .Where(r => r.Date == today && statusNames.Contains(r.Status.Name))
            .ToListAsync();

        var queue = await context.Queue
            .Where(q => q.QueueCode != null)
            .ToListAsync();


        var result = windowCategories
            .GroupBy(wc => wc.Window.WindowNumber)
            .Select(g =>
            {
                var windowId = g.First().WindowID;

                var clients = reservations
                    .Where(r =>
                        (r.Status.Name == "Oczekujący" &&
                         g.Any(wc => wc.CategoryID == r.Category?.ID) &&
                         queue.Any(q => q.ReservationID == r.ID)) ||
                        (r.Status.Name is "Wezwany" or "Obsługiwany" &&
                         queue.Any(q => q.ReservationID == r.ID && q.WindowID == windowId))
                    )
                    .OrderBy(r => r.Time)
                    .Select(r =>
                    {
                        var q = r.Status.Name == "Oczekujący"
                            ? queue.FirstOrDefault(q => q.ReservationID == r.ID)
                            : queue.FirstOrDefault(q => q.ReservationID == r.ID && q.WindowID == windowId);

                        if (q == null || string.IsNullOrWhiteSpace(q.QueueCode) || r.Category == null ||
                            r.Status == null)
                            return null;

                        return new ClientDto
                        {
                            QueueCode = q.QueueCode,
                            FullName = $"{r.Client?.FirstName ?? " "} {r.Client?.LastName ?? " "}",
                            Category = r.Category.ID,
                            Status = r.Status.ID
                        };
                    })
                    .Where(c => c != null)
                    .ToList();

                return new WindowQueueDto
                {
                    WindowNumber = g.Key,
                    Clients = clients!
                };
            })
            .Where(w => w.Clients.Any())
            .ToList();

        if (!result.Any())
            return NotFound("Brak klientów do wyświetlenia.");

        return Ok(result);
    }
}
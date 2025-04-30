using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Models;
using WebApi.Models.DTOs.Queue;

namespace WebApi.Controllers;

[ApiController]
[Route("api/queue")]
public class QueueController(ApplicationDbContext context) : ControllerBase
{
    [HttpGet("window/{windowId}")]
    [Authorize(Roles = "Administrator, Urzędnik")]
    [ProducesResponseType(typeof(List<ClientDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetWindowQueue(int windowId)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Console.WriteLine(userIdString);
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
                wc.CategoryID,
                wc.Category.Name
            })
            .FirstOrDefaultAsync();

        if (categoryInfo == null)
        {
            return StatusCode(403, "Brak uprawnień do tego okienka lub kategorii");
        }


        var waitingClients = await context.Reservations
            .Where(r => r.CategoryID == categoryInfo.CategoryID &&
                        r.Status.Name == "Oczekujący")
            .OrderBy(r => r.Time)
            .Select(r => new ClientDto
            {
                QueueCode = r.ConfirmationCode ?? "BRAK_KODU",
                FullName = $"{r.Client.FirstName} {r.Client.LastName}",
                Category = categoryInfo.Name,
                Status = "Oczekujący"
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
    public async Task<IActionResult> GetAllClientsPerWindow()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var targetStatuses = new[] { "Oczekujący", "Wezwany", "Obsługiwany" };


        var windowCategories = await context.Windows_and_Categories
            .Where(wc => wc.Date == today)
            .Include(wc => wc.Window)
            .Include(wc => wc.Category)
            .ToListAsync();


        var reservations = await context.Reservations
            .Include(r => r.Client)
            .Include(r => r.Status)
            .Where(r => r.Date == today && targetStatuses.Contains(r.Status.Name))
            .ToListAsync();


        var result = windowCategories
            .GroupBy(wc => wc.Window.WindowNumber)
            .Select(g => new WindowQueueDto
            {
                WindowNumber = g.Key,
                Clients = reservations
                    .Where(r => g.Any(wc => wc.CategoryID == r.CategoryID))
                    .OrderBy(r => r.Time)
                    .Select(r => new ClientDto
                    {
                        QueueCode = r.ConfirmationCode ?? "BRAK_KODU",
                        FullName = $"{r.Client.FirstName} {r.Client.LastName}",
                        Category = g.First(wc => wc.CategoryID == r.CategoryID).Category.Name,
                        Status = r.Status.Name
                    })
                    .ToList()
            })
            .Where(w => w.Clients.Any())
            .ToList();

        return Ok(result);
    }
}
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Models.Entities;

namespace WebApi.Controllers;

[ApiController]
[Route("api/clerk/{id}")]
public class ClerkController(ApplicationDbContext context) : ControllerBase
{
    [HttpPut("nextCase")]
    [Authorize(Roles = "Administrator, Urzędnik")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CallNextClient([FromRoute] int id)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);

        var windowAndCategory = await context.Windows_and_Categories
            .FirstOrDefaultAsync(wc => wc.ClerkID == id && wc.Date == today);

        if (windowAndCategory == null)
            return NotFound("Clerk nie ma przypisanej kategorii na dzisiaj.");

        var categoryId = windowAndCategory.CategoryID;
        var windowId = windowAndCategory.WindowID;

        var activeStatusNames = new[] { "Wezwany", "Obsługiwany" };

        var isBusy = await context.Queue
            .Include(q => q.Reservation)
            .ThenInclude(r => r.Status)
            .AnyAsync(q =>
                q.WindowID == windowId &&
                q.Reservation != null &&
                q.Reservation.Status != null &&
                activeStatusNames.Contains(q.Reservation.Status.Name));

        if (isBusy)
            return BadRequest("Urzędnik już obsługuje klienta.");

        var awaitingStatusId = await context.Status
            .Where(s => s.Name == "Oczekujący")
            .Select(s => s.ID)
            .FirstOrDefaultAsync();

        if (awaitingStatusId == 0)
            return StatusCode(StatusCodes.Status500InternalServerError, "Nie znaleziono statusu 'Oczekujący'.");

        var reservation = await context.Reservations
            .Where(r => r.CategoryID == categoryId &&
                        r.StatusID == awaitingStatusId &&
                        r.Date == today)
            .OrderBy(r => r.Time)
            .FirstOrDefaultAsync();

        if (reservation == null)
            return NotFound("Brak oczekujących klientów w tej kategorii.");

        var queueEntry = await context.Queue
            .FirstOrDefaultAsync(q => q.ReservationID == reservation.ID);

        if (queueEntry == null)
            return StatusCode(StatusCodes.Status500InternalServerError, "Brakuje wpisu w kolejce dla tej rezerwacji.");

        queueEntry.WindowID = windowId;

        var calledStatusId = await context.Status
            .Where(s => s.Name == "Wezwany")
            .Select(s => s.ID)
            .FirstOrDefaultAsync();

        if (calledStatusId == 0)
            return StatusCode(StatusCodes.Status500InternalServerError, "Status 'Wezwany' nie istnieje.");

        reservation.StatusID = calledStatusId;

        await context.SaveChangesAsync();

        return Ok(new { caseId = reservation.ID });
    }


    [HttpPut("finishCase/{caseId}")]
    [Authorize(Roles = "Administrator, Urzędnik")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> FinishCase(int caseId)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdString, out var clerkId))
        {
            return Forbid("Nie można odczytać ID zalogowanego użytkownika.");
        }

        var userRole = User.FindFirstValue(ClaimTypes.Role);

        var reservation = await context.Reservations
            .Include(r => r.Status)
            .FirstOrDefaultAsync(r => r.ID == caseId);

        if (reservation == null)
            return NotFound($"Rezerwacja o ID {caseId} nie została znaleziona.");

        if (reservation.Status?.Name != "Obsługiwany")
            return BadRequest("Rezerwację można zakończyć tylko, gdy jest w statusie 'Obsługiwany'.");

        var queueEntry = await context.Queue
            .FirstOrDefaultAsync(q => q.ReservationID == reservation.ID);

        if (queueEntry == null || queueEntry.WindowID == null)
            return Forbid("Rezerwacja nie jest przypisana do żadnego okienka.");

        if (userRole != "Administrator")
        {
            var isAssignedToThisWindow = await context.Windows_and_Categories
                .AnyAsync(wc =>
                    wc.ClerkID == clerkId &&
                    wc.WindowID == queueEntry.WindowID &&
                    wc.Date == today);

            if (!isAssignedToThisWindow)
                return StatusCode(StatusCodes.Status403Forbidden,
                    "Nie masz uprawnień do zakończenia tej sprawy — klient został przypisany do innego okienka.");
        }

        var completedStatus = await context.Status
            .FirstOrDefaultAsync(s => s.Name == "Obsłużony");

        if (completedStatus == null)
            return BadRequest("Status 'Obsłużony' nie istnieje w systemie.");

        reservation.StatusID = completedStatus.ID;
        context.Queue.Remove(queueEntry);

        await context.SaveChangesAsync();

        return Ok(new { message = "Sprawa zakończona i usunięta z kolejki.", caseId = reservation.ID });
    }


    [HttpPut("startService/{caseId}")]
    [Authorize(Roles = "Administrator, Urzędnik")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> StartService(int caseId)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdString, out var clerkId))
        {
            return Forbid();
        }

        var userRole = User.FindFirstValue(ClaimTypes.Role);

        var reservation = await context.Reservations
            .Include(r => r.Status)
            .FirstOrDefaultAsync(r => r.ID == caseId);

        if (reservation == null)
            return NotFound($"Rezerwacja o ID {caseId} nie została znaleziona.");

        if (reservation.Status?.Name != "Wezwany")
            return BadRequest("Rezerwacja może zostać oznaczona jako 'Obsługiwany' tylko po wcześniejszym wezwaniu.");

        var queueEntry = await context.Queue
            .FirstOrDefaultAsync(q => q.ReservationID == reservation.ID);

        if (queueEntry == null || queueEntry.WindowID == null)
            return StatusCode(StatusCodes.Status403Forbidden, "Rezerwacja nie jest przypisana do żadnego okienka.");

        if (userRole != "Administrator")
        {
            var isAssignedToThisWindow = await context.Windows_and_Categories
                .AnyAsync(wc =>
                    wc.ClerkID == clerkId &&
                    wc.WindowID == queueEntry.WindowID &&
                    wc.Date == today);

            if (!isAssignedToThisWindow)
                return StatusCode(StatusCodes.Status403Forbidden,
                    "Nie masz uprawnień do rozpoczęcia obsługi tej rezerwacji.");
        }

        var inServiceStatus = await context.Status
            .FirstOrDefaultAsync(s => s.Name == "Obsługiwany");

        if (inServiceStatus == null)
            return BadRequest("Status 'Obsługiwany' nie istnieje w systemie.");

        reservation.StatusID = inServiceStatus.ID;

        await context.SaveChangesAsync();

        return Ok(new { message = "Rezerwacja zmieniona na 'Obsługiwany'.", caseId = reservation.ID });
    }


    [HttpPut("cancelService/{caseId}")]
    [Authorize(Roles = "Administrator, Urzędnik")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CancelService(int caseId)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdString, out var clerkId))
        {
            return Forbid();
        }

        var userRole = User.FindFirstValue(ClaimTypes.Role);

        var reservation = await context.Reservations
            .Include(r => r.Status)
            .FirstOrDefaultAsync(r => r.ID == caseId);

        if (reservation == null)
            return NotFound($"Rezerwacja o ID {caseId} nie została znaleziona.");

        var queueEntry = await context.Queue
            .FirstOrDefaultAsync(q => q.ReservationID == reservation.ID);

        if (queueEntry == null || queueEntry.WindowID == null)
            return StatusCode(StatusCodes.Status403Forbidden, "Rezerwacja nie jest przypisana do żadnego okienka.");

        if (userRole != "Administrator")
        {
            var isAssignedToThisWindow = await context.Windows_and_Categories
                .AnyAsync(wc =>
                    wc.ClerkID == clerkId &&
                    wc.WindowID == queueEntry.WindowID &&
                    wc.Date == today);

            if (!isAssignedToThisWindow)
                return StatusCode(StatusCodes.Status403Forbidden, "Nie masz uprawnień do anulowania tej rezerwacji.");
        }

        var canceledStatus = await context.Status
            .FirstOrDefaultAsync(s => s.Name == "Odwołany");

        if (canceledStatus == null)
            return BadRequest("Status 'Odwołany' nie istnieje w systemie.");

        reservation.StatusID = canceledStatus.ID;
        context.Queue.Remove(queueEntry);

        await context.SaveChangesAsync();

        return Ok(new
        {
            message = "Rezerwacja została oznaczona jako 'Odwołany' i usunięta z kolejki.",
            caseId = reservation.ID
        });
    }

    [HttpGet("window")]
    [Authorize(Roles = "Administrator, Urzędnik")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTodayWindow(string id)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (!int.TryParse(userIdString, out var clerkId)) return BadRequest("Invalid user ID in token.");
        if (userIdString != id) return BadRequest("Mismatch between IDs in token and request.");

        var user = await context.Users.FindAsync(clerkId);
        if (user == null) return NotFound("Clerk not found.");

        var today = DateOnly.FromDateTime(DateTime.Now);

        var windowId = await context.Windows_and_Categories
            .Where(wc => wc.ClerkID == clerkId && wc.Date == today)
            .Select(wc => wc.WindowID)
            .FirstOrDefaultAsync();

        return windowId == null ? NotFound("Window for clerk not found.") : Ok(new { windowID = windowId });
    }
}
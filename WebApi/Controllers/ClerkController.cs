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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CallNextClient([FromRoute] int id)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);


        var windowAndCategory = await context.Windows_and_Categories
            .Where(wc => wc.ClerkID == id && wc.Date == today)
            .FirstOrDefaultAsync();

        if (windowAndCategory == null)
        {
            return NotFound("Clerk nie ma przypisanej kategorii na dzisiaj.");
        }

        var categoryId = windowAndCategory.CategoryID;
        var windowId = windowAndCategory.WindowID;


        var awaitingStatusId = await context.Status
            .Where(s => s.Name == "Oczekujący")
            .Select(s => s.ID)
            .FirstOrDefaultAsync();

        if (awaitingStatusId == 0)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Nie znaleziono statusu 'Oczekujący'.");
        }

        var reservation = await context.Reservations
            .Where(r => r.CategoryID == categoryId && r.StatusID == awaitingStatusId && r.Date == today)
            .OrderBy(r => r.Time)
            .FirstOrDefaultAsync();

        if (reservation == null)
        {
            return NotFound("Brak oczekujących klientów w tej kategorii.");
        }


        var queueEntry = await context.Queue
            .Where(q => q.ReservationID == reservation.ID)
            .FirstOrDefaultAsync();

        if (queueEntry == null)
        {
            queueEntry = new Queue
            {
                ReservationID = reservation.ID,
                WindowID = windowId,
                QueueCode = null
            };
            context.Queue.Add(queueEntry);
        }
        else
        {
            queueEntry.WindowID = windowId;
        }


        var calledStatusId = await context.Status
            .Where(s => s.Name == "Wezwany")
            .Select(s => s.ID)
            .FirstOrDefaultAsync();

        if (calledStatusId != 0)
        {
            reservation.StatusID = calledStatusId;
        }

        await context.SaveChangesAsync();

        return Ok();
    }


    [HttpPut("finishCase/{caseId}")]
    [Authorize(Roles = "Administrator, Urzędnik")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> FinishCase(int caseId)
    {
        var reservation = await context.Reservations
            .FirstOrDefaultAsync(r => r.ID == caseId);

        if (reservation == null)
        {
            return NotFound($"Rezerwacja o ID {caseId} nie została znaleziona.");
        }

        var completedStatus = await context.Status
            .FirstOrDefaultAsync(s => s.Name == "Obsłużony");

        if (completedStatus == null)
        {
            return BadRequest("Status 'Obsłużony' nie istnieje w systemie.");
        }


        reservation.StatusID = completedStatus.ID;


        var queueEntry = await context.Queue
            .FirstOrDefaultAsync(q => q.ReservationID == reservation.ID);

        if (queueEntry != null)
        {
            context.Queue.Remove(queueEntry);
        }

        await context.SaveChangesAsync();

        return Ok("Sprawa zakończona i usunięta z kolejki.");
    }

    [HttpPut("startService/{caseId}")]
    [Authorize(Roles = "Administrator, Urzędnik")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> StartService(int caseId)
    {
        var reservation = await context.Reservations
            .FirstOrDefaultAsync(r => r.ID == caseId);

        if (reservation == null)
        {
            return NotFound($"Rezerwacja o ID {caseId} nie została znaleziona.");
        }

        var inServiceStatus = await context.Status
            .FirstOrDefaultAsync(s => s.Name == "Obsługiwany");

        if (inServiceStatus == null)
        {
            return BadRequest("Status 'Obsługiwany' nie istnieje w systemie.");
        }

        reservation.StatusID = inServiceStatus.ID;

        await context.SaveChangesAsync();

        return Ok("Rezerwacja zmieniona na 'Obsługiwany'.");
    }

    [HttpPut("cancelService/{caseId}")]
    [Authorize(Roles = "Administrator, Urzędnik")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelService(int caseId)
    {
        var reservation = await context.Reservations
            .FirstOrDefaultAsync(r => r.ID == caseId);

        if (reservation == null)
        {
            return NotFound($"Rezerwacja o ID {caseId} nie została znaleziona.");
        }

        var canceledStatus = await context.Status
            .FirstOrDefaultAsync(s => s.Name == "Odwołany");

        if (canceledStatus == null)
        {
            return BadRequest("Status 'Odwołany' nie istnieje w systemie.");
        }

        reservation.StatusID = canceledStatus.ID;


        var queueEntry = await context.Queue
            .FirstOrDefaultAsync(q => q.ReservationID == reservation.ID);

        if (queueEntry != null)
        {
            context.Queue.Remove(queueEntry);
        }

        await context.SaveChangesAsync();

        return Ok("Rezerwacja została oznaczona jako 'Odwołany' i usunięta z kolejki.");
    }
}
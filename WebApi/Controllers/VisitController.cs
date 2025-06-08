using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Helpers;
using WebApi.Models.DTOs.Visit;
using WebApi.Models.Entities;

namespace WebApi.Controllers;

[ApiController]
[Route("api/visit")]
public class VisitController(ApplicationDbContext context, IMapper mapper, EmailService emailService) : ControllerBase
{
    private async Task<bool> AddReservationToQueue(Reservation reservation, string registrationNumber)
    {
        try
        {
            var queueEntry = new Queue
            {
                ReservationID = reservation.ID,
                QueueCode = registrationNumber
            };

            context.Queue.Add(queueEntry);
            await context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] AddReservationToQueue: {ex.Message}");
            return false;
        }
    }
    
    private static List<object> GetInfoList()
    {
        var list = new List<object>
        {
            new { 
                title = "Zjaw się przed czasem", 
                description = "Przyjdź na miejsce przynajmniej 10 minut przed zaplanowaną godziną wizyty, " +
                              "aby mieć czas potwierdzić swoją obecność kodem otrzymanym podczas rejestracji"
            },
            new
            {
                title = "Potwierdź swoje przybycie", 
                description = "Za pomocą otrzymanego kodu, potwierdź swoje przybycie do urzędu, " +
                              "zrobisz to w terminalu znajdującym się przy wejściu. " +
                              "UWAGA: Jeśli nie potwierdzisz przybycia minimum 5 minut przed godziną wizyty, " +
                              "Twoja rezerwacja może zostać anulowana."
            },
            new
            {
                title = "Czekaj na wezwanie", 
                description = "Po potwierdzeniu wizyty kodem otrzymanym podczas rejestracji, otrzymasz numerek kolejkowy. " +
                              "Gdy nadejdzie Twoja kolej, na ekranie zobaczysz swój numer."
            }
        };

        return list;
    }
    
    [HttpPost("schedule")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(VisitResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    [Produces("application/json")]
    public async Task<IActionResult> ScheduleVisit([FromBody] VisitRequest visitRequest, [FromHeader(Name = "X-Frontend-Url")] string url)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        
        if (string.IsNullOrWhiteSpace(url) || !Uri.IsWellFormedUriString(url, UriKind.Absolute))
            return BadRequest("No valid 'X-Frontend-BaseUrl' header in request.");
        
        var today = DateOnly.FromDateTime(DateTime.Now);
        var nowTime = TimeOnly.FromDateTime(DateTime.Now);
        
        if (visitRequest.Date < today) 
            return BadRequest("You cannot schedule a visit in the past.");
        
        if (visitRequest.Date == today && visitRequest.Time <= nowTime)
                return BadRequest("Visit time must be later than the current time.");
        
        var category = await context.CaseCategories.FirstOrDefaultAsync(c => c.ID == visitRequest.CaseCategoryID);
        if (category == null) return BadRequest("Invalid Case Category ID.");
        
        var slot = await context.Slots.FirstOrDefaultAsync(s => s.CategoryID == category.ID
                                                                && s.Date == visitRequest.Date 
                                                                && s.Time == visitRequest.Time);
        
        if (slot == null || slot.CurrentReservations >= slot.MaxReservations) 
            return BadRequest("No available spots for the selected date.");
        
        var client = await context.Clients.FirstOrDefaultAsync(c => c.PESEL == visitRequest.Pesel);
        if (client == null)
        {
            client = mapper.Map<Client>(visitRequest);
            context.Clients.Add(client);
            await context.SaveChangesAsync();
        }
        else
        {
            var updated = false;

            if (client.FirstName != visitRequest.FirstName) { client.FirstName = visitRequest.FirstName; updated = true; }
            if (client.LastName != visitRequest.LastName) { client.LastName = visitRequest.LastName; updated = true; }
            if (client.Email != visitRequest.Email) { client.Email = visitRequest.Email; updated = true; }
            if (client.Phone != visitRequest.Phone) { client.Phone = visitRequest.Phone; updated = true; }

            if (updated) await context.SaveChangesAsync();
        }
        
        var existingReservation = await context.Reservations
            .FirstOrDefaultAsync(r =>
                r.ClientID == client.ID &&
                r.StatusID != 3 &&
                r.Date == visitRequest.Date &&
                r.Time == visitRequest.Time);

        if (existingReservation != null)
            return BadRequest("Client already has a reservation for the selected date and time.");
        
        var reservation = mapper.Map<Reservation>(visitRequest);
        reservation.ClientID = client.ID;
        reservation.CategoryID = category.ID;
        reservation.Category = category;
        reservation.StatusID = 1;
        
        context.Reservations.Add(reservation);
        slot.CurrentReservations++;
        await context.SaveChangesAsync();
        
        var cancellationLink = $"{url.TrimEnd('/')}/visit/cancel/{reservation.ID}";
        
        var visitResponse = mapper.Map<VisitResponse>(reservation);
        visitResponse.Email = visitRequest.Email;
        visitResponse.CancellationLink = cancellationLink;
        
        try
        {
            await emailService.SendConfirmationEmailAsync(client, reservation, category, cancellationLink);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(500, "An error occurred while sending the confirmation email.");
        }

        return Ok(visitResponse);
    }
    
    [HttpPost("cancel/{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [Produces("application/json")]
    public async Task<IActionResult> CancelVisit(int id)
    {
        var reservation = await context.Reservations.FindAsync(id);

        if (reservation == null) return NotFound("Reservation not found.");
        if (reservation.StatusID == 3) return BadRequest("Reservation is already cancelled.");
        
        var client = await context.Clients.FirstOrDefaultAsync(c => c.ID == reservation.ClientID);
        if (client == null) return StatusCode(500, "An error occurred while searching for the client.");
        
        var category = await context.CaseCategories.FirstOrDefaultAsync(c => c.ID == reservation.CategoryID);
        if (category == null) return StatusCode(500, "An error occurred while searching for the category.");
        
        var slot = await context.Slots.FirstOrDefaultAsync(s =>
            s.CategoryID == reservation.CategoryID &&
            s.Date == reservation.Date &&
            s.Time == reservation.Time);

        if (slot == null) return StatusCode(500, "Slot corresponding to the reservation was not found.");

        if (slot.CurrentReservations > 0) 
            slot.CurrentReservations--;
        
        reservation.StatusID = 3;
        await context.SaveChangesAsync();
        
        try
        {
            await emailService.SendCancellationEmailAsync(client, reservation, category);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(500, "An error occurred while sending the cancellation email.");
        }

        return Ok("Pomyślnie odwołano rezerwację wizyty. Potwierdzenie zostało wysłane na podany adres e-mail podczas rezerwacji.");
    }
    
    [HttpGet("details/{visitId}")]
    [Authorize]
    [ProducesResponseType(typeof(VisitDetailsDto), 200)]
    [ProducesResponseType(404)]
    [Produces("application/json")]
    public async Task<IActionResult> GetVisitDetails(int visitId)
    {
        var reservation = await context.Reservations
            .Include(r => r.Client)
            .Include(r => r.Category)
            .Include(r => r.Status)
            .FirstOrDefaultAsync(r => r.ID == visitId);

        if (reservation == null) return NotFound("Visit not found.");

        var visitDetails = mapper.Map<VisitDetailsDto>(reservation);

        return Ok(visitDetails);
    }

    [HttpPost("validateCode")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [Produces("application/json")]
    public async Task<IActionResult> ValidateCode([FromBody] string confirmationCode)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var reservation = await context.Reservations
            .Include(r => r.Category)
            .FirstOrDefaultAsync(r =>
                r.Date.HasValue && r.Date.Value == today &&
                r.ConfirmationCode == confirmationCode);
        
        if (reservation == null) return BadRequest("Invalid or expired confirmation code for today's reservations.");
        reservation.StatusID = 2;
        
        var letter = reservation.Category.Letter;
        var registrationNumber = RegistrationNumberGenerator.Generate(letter);

        await context.SaveChangesAsync();
        
        var added = await AddReservationToQueue(reservation, registrationNumber);
        return !added ? StatusCode(500, "An error occurred while adding to the queue.") : Ok(new { RegistrationNumber = registrationNumber });
    }

    [HttpPost("unregistered/schedule")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [Produces("application/json")]
    public async Task<IActionResult> ScheduleUnregistered([FromBody] UnregisteredVisitRequest request)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var time = new TimeOnly((DateTime.Now.Hour + 1) % 24, 0);
        
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var category = await context.CaseCategories.FirstOrDefaultAsync(c => c.ID == request.CaseCategoryID);
        if (category == null) return BadRequest("Invalid Case Category ID.");

        var slot = await context.Slots.FirstOrDefaultAsync(s => s.CategoryID == category.ID 
                                                                && s.Date == today 
                                                                && s.Time == time);

        if (slot == null || slot.CurrentReservations >= slot.MaxReservations)
            return BadRequest("No available slots for the selected time.");
        
        var status = await context.Status.FirstOrDefaultAsync(s => s.ID == 2);

        var reservation = new Reservation {
            CategoryID = category.ID, 
            Category = category,
            Status = status,
            StatusID = 2,
            Date = today,
            Time = time
        };
        
        context.Reservations.Add(reservation);
        slot.CurrentReservations++;
        await context.SaveChangesAsync();
        
        var registrationNumber = RegistrationNumberGenerator.Generate(category.Letter);
        
        var added = await AddReservationToQueue(reservation, registrationNumber);
        return !added ? StatusCode(500, "An error occurred while adding to the queue.") : Ok(new { RegistrationNumber = registrationNumber });
    }
    
    [HttpGet("info")]
    [ProducesResponseType(200)]
    [Produces("application/json")]
    public IActionResult GetInfo()
    {
        return Ok(GetInfoList());
    }

    [HttpGet("slots/{categoryId}")]
    [ProducesResponseType(typeof(List<SlotDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [Produces("application/json")]
    public async Task<IActionResult> GetAvailableSlotsByCategory(int categoryId)
    {
        var category = await context.CaseCategories.FirstOrDefaultAsync(c => c.ID == categoryId);
        if (category == null) return BadRequest("Invalid Case Category ID.");
        
        var today = DateOnly.FromDateTime(DateTime.Now);
        var nowTime = TimeOnly.FromDateTime(DateTime.Now);
        
        var slots = await context.Slots
            .Where(s => s.CategoryID == categoryId && 
                        s.CurrentReservations < s.MaxReservations &&
                        (
                            s.Date > today || 
                            (s.Date == today && s.Time > nowTime)
                        )
            )
            .OrderBy(s => s.Date)
            .ThenBy(s => s.Time)
            .ToListAsync();

        var slotDtos = mapper.Map<List<SlotDto>>(slots);

        return Ok(slotDtos);
    }

}

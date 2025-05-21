using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Models.DTOs.Category;
using WebApi.Models.Entities;

namespace WebApi.Controllers;

[ApiController]
[Route("api/category")]
public class CategoryController(ApplicationDbContext context) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Administrator, Urzędnik")]
    public async Task<ActionResult<IEnumerable<CaseCategory>>> GetCategories()
    {
        return await context.CaseCategories.ToListAsync();
    }


    [HttpGet("{id}")]
    [Authorize(Roles = "Administrator, Urzędnik")]
    public async Task<ActionResult<CaseCategory>> GetCategory(int id)
    {
        var category = await context.CaseCategories.FindAsync(id);

        if (category == null)
        {
            return NotFound();
        }

        return category;
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(CaseCategory), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCaseCategoryDto categoryDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var letterExists = await context.CaseCategories
            .AnyAsync(c => c.Letter == categoryDto.Letter);

        if (letterExists)
        {
            return Conflict($"Category with letter '{categoryDto.Letter}' already exists.");
        }

        var nameExists = await context.CaseCategories
            .AnyAsync(c => c.Name == categoryDto.Name);

        if (nameExists)
        {
            return Conflict($"Category with name '{categoryDto.Name}' already exists.");
        }

        var category = new CaseCategory
        {
            Letter = categoryDto.Letter,
            Name = categoryDto.Name
        };

        context.CaseCategories.Add(category);
        await context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetCategory),
            new { id = category.ID },
            category);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCaseCategoryDto categoryDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existingCategory = await context.CaseCategories.FindAsync(id);
        if (existingCategory == null)
        {
            return NotFound();
        }
        
        if (string.IsNullOrEmpty(categoryDto.Letter) &&
            string.IsNullOrEmpty(categoryDto.Name))
        {
            return BadRequest("No update data provided.");
        }

        if (!string.IsNullOrEmpty(categoryDto.Letter) &&
            categoryDto.Letter != existingCategory.Letter)
        {
            var letterExists = await context.CaseCategories
                .AnyAsync(c => c.Letter == categoryDto.Letter && c.ID != id);

            if (letterExists)
            {
                return Conflict($"Category with letter '{categoryDto.Letter}' already exists.");
            }

            existingCategory.Letter = categoryDto.Letter;
        }

        if (!string.IsNullOrEmpty(categoryDto.Name) &&
            categoryDto.Name != existingCategory.Name)
        {
            var nameExists = await context.CaseCategories
                .AnyAsync(c => c.Name == categoryDto.Name && c.ID != id);

            if (nameExists)
            {
                return Conflict($"Category with name '{categoryDto.Name}' already exists.");
            }

            existingCategory.Name = categoryDto.Name;
        }

        context.CaseCategories.Update(existingCategory);
        await context.SaveChangesAsync();

        return Ok(existingCategory);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await context.CaseCategories.FindAsync(id);
        if (category == null)
        {
            return NotFound();
        }

        context.CaseCategories.Remove(category);
        await context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("{categoryId}/queue")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(IEnumerable<ReservationQueueDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetQueueForClerkCategory(int categoryId)
    {
        var category = await context.CaseCategories
            .FirstOrDefaultAsync(c => c.ID == categoryId);

        if (category == null)
        {
            return NotFound($"Category with ID {categoryId} not found.");
        }

        var queue = await context.Queue
            .Include(q => q.Reservation)
            .ThenInclude(r => r.Client)
            .Include(q => q.Reservation)
            .ThenInclude(r => r.Status)
            .Include(q => q.Window)
            .Where(q => q.Reservation.CategoryID == categoryId &&
                        q.Reservation.Status.Name == "Oczekujący")
            .OrderBy(q => q.Reservation.Time)
            .Select(q => new ReservationQueueDto
            {
                FirstName = q.Reservation.Client.FirstName,
                LastName = q.Reservation.Client.LastName,
                PESEL = q.Reservation.Client.PESEL,
            })
            .ToListAsync();

        return Ok(queue);
    }
}
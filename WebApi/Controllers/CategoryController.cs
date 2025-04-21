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
    public async Task<ActionResult<IEnumerable<CaseCategory>>> GetCategories()
    {
        return await context.CaseCategories.ToListAsync();
    }
    
    [HttpGet("{id}")]
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
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] CaseCategory category)
    {
        // TODO: Check if the category exists by id
        // TODO: Update the category properties (e.g., name, etc.)

        return NoContent(); // TODO: delete this placeholder
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        // TODO: Find the category by id
        // TODO: Delete the category from the database

        return NoContent(); // TODO: delete this placeholder
    }
    
    [HttpGet("{categoryId}/queue")]
    public async Task<IActionResult> GetQueueForClerkCategory(int categoryId)
    {
        // TODO: Find the category by categoryID
        // TODO: Get all clients waiting for the same case category as the clerk
        // TODO: Return the client queue
        return NoContent(); // TODO: delete this placeholder
    }
}
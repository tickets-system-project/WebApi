using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
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
    public async Task<IActionResult> CreateCategory([FromBody] CaseCategory category)
    {
        // TODO: Validate the Category object (e.g., name, etc.)
        // TODO: Add the new category to the database

        return NoContent(); // TODO: delete this placeholder
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
using Microsoft.EntityFrameworkCore;
using WebApi.Models.Entities;

namespace WebApi.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    
    public DbSet<Role> Roles { get; set; }
    public DbSet<CaseCategory> CaseCategories { get; set; }
    public DbSet<Window> Windows { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<Slot> Slots { get; set; }
    public DbSet<Window_and_Category> Windows_and_Categories { get; set; }
    public DbSet<Queue> Queue { get; set; }
    public DbSet<LoginData> LoginData { get; set; }
}

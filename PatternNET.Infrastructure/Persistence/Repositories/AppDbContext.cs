namespace PatternNET.Contact.Infrastructure.Persistence;

/// <summary>
/// DB CONTEXT EF CORE
/// 
/// TEMPAT:
/// - Mapping ke database
/// - BUKAN business logic
/// </summary>
public class AppDbContext : DbContext
{
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
}
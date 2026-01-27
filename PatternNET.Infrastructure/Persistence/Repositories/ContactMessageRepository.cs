namespace PatternNET.Contact.Infrastructure.Persistence.Repositories;

using PatternNET.Contact.Application.Interfaces;
using PatternNET.Contact.Domain.Entities;

/// <summary>
/// IMPLEMENTASI REPOSITORY
/// 
/// DI SINI:
/// - EF Core
/// - LINQ
/// - SQL
/// </summary>
public class ContactMessageRepository : IContactMessageRepository
{
    private readonly AppDbContext _context;

    public ContactMessageRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(ContactMessage message)
    {
        _context.ContactMessages.Add(message);
        await _context.SaveChangesAsync();
    }
}
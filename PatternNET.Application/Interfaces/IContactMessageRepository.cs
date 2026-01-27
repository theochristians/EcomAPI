namespace PatternNET.Contact.Application.Interfaces;

using PatternNET.Contact.Domain.Entities;

/// <summary>
/// KONTRAK PENYIMPANAN
/// 
/// Application TIDAK PEDULI:
/// - pakai EF
/// - pakai SQL
/// - pakai API
/// 
/// Yang penting: bisa SIMPAN ContactMessage
/// </summary>
public interface IContactMessageRepository
{
    Task AddAsync(ContactMessage message);
}
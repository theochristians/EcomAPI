namespace PatternNET.Contact.Domain.Exceptions;

/// <summary>
/// EXCEPTION KHUSUS DOMAIN
/// Digunakan untuk error ATURAN BISNIS.
/// 
/// Contoh:
/// - Email tidak valid
/// - Order sudah dibayar
/// 
/// BUKAN error teknis (DB, Network, dll)
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
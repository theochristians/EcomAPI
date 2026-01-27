namespace PatternNET.Contact.Domain.Entities;

/// <summary>
/// ENTITY UTAMA DOMAIN
/// Mewakili konsep "Contact Message" dalam BISNIS.
/// 
/// TUGAS:
/// - Menjaga aturan bisnis
/// - Menolak data tidak valid
/// - TIDAK tahu DB
/// - TIDAK tahu HTTP
/// </summary>
public class ContactMessage
{
    public int Id { get; private set; }

    public string Name { get; private set; }
    public Email Email { get; private set; }
    public string Subject { get; private set; }
    public string Message { get; private set; }

    /// <summary>
    /// CONSTRUCTOR DOMAIN
    /// Semua aturan WAJIB dicek di sini
    /// </summary>
    public ContactMessage(string name, Email email, string subject, string message)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Name is required");

        Name = name;
        Email = email;
        Subject = subject;
        Message = message;
    }

    /// <summary>
    /// Dipanggil oleh Infrastructure SETELAH data tersimpan di DB
    /// Domain tidak tahu bagaimana ID dibuat
    /// </summary>
    public void SetId(int id)
    {
        Id = id;
    }
}
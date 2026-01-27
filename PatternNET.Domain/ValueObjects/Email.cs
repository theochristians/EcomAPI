namespace PatternNET.Contact.Domain.ValueObjects;

/// <summary>
/// VALUE OBJECT
/// Digunakan untuk menyimpan nilai yang PUNYA aturan,
/// tapi TIDAK punya identitas.
/// 
/// Contoh VO:
/// - Email
/// - Money
/// - PhoneNumber
/// </summary>
public class Email
{
    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Email is required");

        if (!value.Contains("@"))
            throw new DomainException("Invalid email format");

        Value = value;
    }
}
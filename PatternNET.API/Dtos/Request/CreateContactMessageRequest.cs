namespace PatternNET.Contact.API.Dtos.Requests;

/// <summary>
/// DTO REQUEST API
/// 
/// Murni kontrak JSON.
/// TIDAK ADA business logic.
/// </summary>
public class CreateContactMessageRequest
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Subject { get; set; }
    public string Message { get; set; }
}
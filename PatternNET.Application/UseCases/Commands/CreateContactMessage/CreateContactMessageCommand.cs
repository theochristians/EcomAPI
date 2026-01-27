namespace PatternNET.Contact.Application.UseCases.Commands.CreateContactMessage;

/// <summary>
/// COMMAND
/// Berisi DATA PERINTAH dari API.
/// 
/// Command TIDAK punya logic.
/// </summary>
public record CreateContactMessageCommand(
    string Name,
    string Email,
    string Subject,
    string Message
);
namespace PatternNET.Contact.Application.UseCases.Commands.CreateContactMessage;

using PatternNET.Contact.Application.Interfaces;
using PatternNET.Contact.Domain.Entities;
using PatternNET.Contact.Domain.ValueObjects;

/// <summary>
/// HANDLER = USE CASE
/// 
/// TUGAS:
/// - Mengatur alur
/// - Membuat Domain Object
/// - Memanggil Repository
/// 
/// BUKAN:
/// - Tempat validasi bisnis detail
/// - Tempat LINQ
/// </summary>
public class CreateContactMessageHandler
{
    private readonly IContactMessageRepository _repository;

    public CreateContactMessageHandler(IContactMessageRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> Handle(CreateContactMessageCommand command)
    {
        var email = new Email(command.Email);

        var message = new ContactMessage(
            command.Name,
            email,
            command.Subject,
            command.Message
        );

        await _repository.AddAsync(message);

        return message.Id;
    }
}
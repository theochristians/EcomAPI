namespace PatternNET.Contact.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using PatternNET.Contact.Application.UseCases.Commands.CreateContactMessage;

/// <summary>
/// CONTROLLER
/// 
/// TUGAS:
/// - Terima HTTP request
/// - Mapping DTO → Command
/// - Return response
/// 
/// TIDAK BOLEH:
/// - Query DB
/// - Validasi bisnis
/// </summary>
[ApiController]
[Route("api/contact")]
public class ContactMessagesController : ControllerBase
{
    private readonly CreateContactMessageHandler _handler;

    public ContactMessagesController(CreateContactMessageHandler handler)
    {
        _handler = handler;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateContactMessageRequest request)
    {
        var command = new CreateContactMessageCommand(
            request.Name,
            request.Email,
            request.Subject,
            request.Message
        );

        var id = await _handler.Handle(command);

        return Ok(new
        {
            success = true,
            data = new { id }
        });
    }
}
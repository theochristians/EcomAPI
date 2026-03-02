namespace EComAPI.Application.Auth.Commands.UpdateAddress
{
    public record UpdateAddressCommand(
        Guid Id,
        string Label,
        string RecipientName,
        string RecipientPhone,
        string FullAddress,
        string City,
        string Province,
        string PostalCode
    );
}

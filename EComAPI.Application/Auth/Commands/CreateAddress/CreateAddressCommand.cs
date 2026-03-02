namespace EComAPI.Application.Auth.Commands.CreateAddress
{
    public record CreateAddressCommand(
        string Label,
        string RecipientName,
        string RecipientPhone,
        string FullAddress,
        string City,
        string Province,
        string PostalCode,
        bool IsDefault
    );
}

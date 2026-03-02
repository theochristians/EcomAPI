namespace EComAPI.API.Auth.Dtos.Request
{
    public record CreateAddressRequest(
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

namespace EComAPI.API.Auth.Dtos.Response
{
    public record AddressResponse(
        Guid Id,
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
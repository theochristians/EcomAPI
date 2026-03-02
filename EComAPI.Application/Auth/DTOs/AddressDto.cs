namespace EComAPI.Application.Auth.DTOs
{
    public record AddressDto(
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

namespace EComAPI.Application.Transaction.DTOs
{
    public record CouponValidationDto(
        bool IsValid,
        decimal Discount,
        string? Message);
}

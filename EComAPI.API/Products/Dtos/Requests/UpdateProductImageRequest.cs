namespace EComAPI.API.Products.Dtos.Requests
{
    public record UpdateProductImageRequest(
        string? ImageUrl,
        bool? IsPrimary,
        int? DisplayOrder
    );
}

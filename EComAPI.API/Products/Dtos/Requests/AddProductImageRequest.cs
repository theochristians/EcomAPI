namespace EComAPI.API.Products.Dtos.Requests
{
    public record AddProductImageRequest(
        string ImageUrl,
        bool IsPrimary = false,
        int DisplayOrder = 0
    );
}
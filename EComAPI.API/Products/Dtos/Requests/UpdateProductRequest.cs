namespace EComAPI.API.Products.Dtos.Requests
{
    public record UpdateProductRequest(
        string? Name,           
        string? Slug,         
        decimal? BasePrice,    
        Guid? CategoryId,       
        string? Description    
    );
}

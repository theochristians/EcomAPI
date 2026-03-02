namespace EComAPI.Application.Products.Queries.GetProducts
{
    public record GetProductsQuery(
        string? CategorySlug = null,
        bool IncludeSubcategories = false,
        bool IncludeInactive = false,
        decimal? MinPrice = null,
        decimal? MaxPrice = null,
        bool? InStock = null,
        string? SearchTerm = null,
        string SortBy = "createdAt",
        string SortOrder = "desc",
        int Page = 1,
        int PageSize = 10
    );
}
namespace EComAPI.Application.Products.Queries.GetProductBySlug
{
    public class GetProductBySlugQuery
    {
        public string Slug { get; }

        public GetProductBySlugQuery(string slug)
        {
            Slug = slug;
        }
    }
}
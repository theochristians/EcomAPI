using EComAPI.API.Categories.Dtos.Requests;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Categories.Swagger.Examples.CreateCategories.Request
{
    public class CreateCategoryRequestExample
        : IExamplesProvider<CreateCategoryRequest>
    {
        public CreateCategoryRequest GetExamples()
            => new(
                "Electronics",
                "electronics",
                null,
                null,
                null
            );
    }
}
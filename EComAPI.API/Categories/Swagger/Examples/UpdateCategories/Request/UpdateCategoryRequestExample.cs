using EComAPI.API.Categories.Dtos.Requests;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Categories.Swagger.Examples.UpdateCategories.Request
{
    public class UpdateCategoryRequestExample
        : IExamplesProvider<UpdateCategoryRequest>
    {
        public UpdateCategoryRequest GetExamples()
            => new(
                "Electronic Devices",
                "electronic-devices",
                null,
                "https://example.com/images/electronic-devices.jpg",
                null
            );
    }
}
using EComAPI.API.Auth.Dtos.Request;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Auth.Swagger.Examples.CreateAddress.Request
{
    public class CreateAddressRequestExample : IExamplesProvider<CreateAddressRequest>
    {
        public CreateAddressRequest GetExamples()
        {
            return new CreateAddressRequest(
                "Rumah",
                "John Doe",
                "081234567890",
                "Jl. Merdeka No. 10",
                "Bandung",
                "Jawa Barat",
                "40123",
                true
            );
        }
    }
}

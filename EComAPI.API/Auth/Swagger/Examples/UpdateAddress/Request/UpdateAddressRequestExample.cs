using EComAPI.API.Auth.Dtos.Request;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Auth.Swagger.Examples.UpdateAddress.Request
{
    public class UpdateAddressRequestExample : IExamplesProvider<UpdateAddressRequest>
    {
        public UpdateAddressRequest GetExamples()
        {
            return new UpdateAddressRequest(
                "Kantor",
                "John Doe",
                "081234567890",
                "Jl. Asia Afrika No. 25",
                "Bandung",
                "Jawa Barat",
                "40211"
            );
        }
    }
}

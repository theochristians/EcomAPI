using EComAPI.API.Auth.Dtos.Response;
using EComAPI.API.Common;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Auth.Swagger.Examples.GetDefaultAddress.Response
{
    public class GetDefaultAddressSuccessExample : IExamplesProvider<ApiResponse<AddressResponse?>>
    {
        public ApiResponse<AddressResponse?> GetExamples()
        {
            return ApiResponse<AddressResponse?>.Ok(
                new AddressResponse(
                    Guid.Parse("9c2f7e31-7e8a-4683-92ef-1c9a5949310a"),
                    "Rumah",
                    "John Doe",
                    "081234567890",
                    "Jl. Merdeka No. 10",
                    "Bandung",
                    "Jawa Barat",
                    "40123",
                    true
                ),
                "Success get default address"
            );
        }
    }
}

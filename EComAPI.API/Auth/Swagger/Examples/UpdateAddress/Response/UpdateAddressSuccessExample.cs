using EComAPI.API.Auth.Dtos.Response;
using EComAPI.API.Common;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Auth.Swagger.Examples.UpdateAddress.Response
{
    public class UpdateAddressSuccessExample : IExamplesProvider<ApiResponse<AddressResponse>>
    {
        public ApiResponse<AddressResponse> GetExamples()
        {
            return ApiResponse<AddressResponse>.Ok(
                new AddressResponse(
                    Guid.Parse("9c2f7e31-7e8a-4683-92ef-1c9a5949310a"),
                    "Kantor",
                    "John Doe",
                    "081234567890",
                    "Jl. Asia Afrika No. 25",
                    "Bandung",
                    "Jawa Barat",
                    "40211",
                    false
                ),
                "Address updated successfully"
            );
        }
    }
}

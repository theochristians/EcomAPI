using EComAPI.API.Auth.Dtos.Response;
using EComAPI.API.Common;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Auth.Swagger.Examples.SetDefaultAddress.Response
{
    public class SetDefaultAddressSuccessExample : IExamplesProvider<ApiResponse<AddressResponse>>
    {
        public ApiResponse<AddressResponse> GetExamples()
        {
            return ApiResponse<AddressResponse>.Ok(
                new AddressResponse(
                    Guid.Parse("f3b6dd8c-59ee-4f4b-b71b-16f2de7ad1e2"),
                    "Kantor",
                    "John Doe",
                    "081234567890",
                    "Jl. Asia Afrika No. 25",
                    "Bandung",
                    "Jawa Barat",
                    "40211",
                    true
                ),
                "Default address updated successfully"
            );
        }
    }
}

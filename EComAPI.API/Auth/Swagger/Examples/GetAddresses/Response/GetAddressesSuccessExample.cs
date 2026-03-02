using EComAPI.API.Auth.Dtos.Response;
using EComAPI.API.Common;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Auth.Swagger.Examples.GetAddresses.Response
{
    public class GetAddressesSuccessExample
        : IExamplesProvider<ApiResponse<IReadOnlyList<AddressResponse>>>
    {
        public ApiResponse<IReadOnlyList<AddressResponse>> GetExamples()
        {
            var addresses = new List<AddressResponse>
            {
                new(
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
                new(
                    Guid.Parse("f3b6dd8c-59ee-4f4b-b71b-16f2de7ad1e2"),
                    "Kantor",
                    "John Doe",
                    "081234567890",
                    "Jl. Asia Afrika No. 25",
                    "Bandung",
                    "Jawa Barat",
                    "40211",
                    false
                )
            };

            return ApiResponse<IReadOnlyList<AddressResponse>>.Ok(
                addresses,
                "Success get addresses"
            );
        }
    }
}

using EComAPI.API.Common;
using EComAPI.API.Shopping.Dtos.Responses;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Shopping.Swagger.Examples.CartExample.GetCart.Response
{
    public class GetCartSuccessExample : IExamplesProvider<ApiResponse<CartResponse>>
    {
        public ApiResponse<CartResponse> GetExamples()
            => ApiResponse<CartResponse>.Ok(
                new CartResponse(
                    Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                    Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901"),
                    new List<CartItemResponse>
                    {
                        new CartItemResponse(
                            Guid.Parse("c3d4e5f6-a7b8-9012-cdef-123456789012"),
                            Guid.Parse("d4e5f6a7-b8c9-0123-defa-234567890123"),
                            "Kaos Polos Premium",
                            "KPP-M-WHT-001",
                            "M",
                            "White",
                            150000m,
                            2,
                            300000m
                        )
                    },
                    300000m,
                    2
                ),
                "Success get cart"
            );
    }
}

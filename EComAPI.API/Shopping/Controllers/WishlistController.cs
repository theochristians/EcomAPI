using EComAPI.API.Common;
using EComAPI.API.Shopping.Dtos.Requests;
using EComAPI.API.Shopping.Dtos.Responses;
using EComAPI.API.Shopping.Swagger.Examples;
using EComAPI.API.Shopping.Swagger.Examples.WishlistExample;
using EComAPI.API.Shopping.Swagger.Examples.WishlistExample.GetWishlist.Response;
using EComAPI.API.Shopping.Swagger.Examples.WishlistExample.AddToWishlist.Request;
using EComAPI.API.Shopping.Swagger.Examples.WishlistExample.AddToWishlist.Response;
using EComAPI.API.Shopping.Swagger.Examples.WishlistExample.RemoveFromWishlist.Response;
using EComAPI.Application.Shopping.Commands.WishlistCommands.AddToWishlist;
using EComAPI.Application.Shopping.Commands.WishlistCommands.RemoveFromWishlist;
using EComAPI.Application.Shopping.Queries.GetWishlist;
using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Shopping.Controllers
{
    /// <summary>
    /// Endpoint untuk mengelola wishlist user.
    /// </summary>
    /// <remarks>
    /// Alur umum (high-level flow):
    /// 1. User menambahkan atau menghapus produk dari wishlist.
    /// 2. Semua endpoint membutuhkan autentikasi.
    /// 3. Controller memetakan request ke command/query Application Layer.
    /// 4. Hasil dikembalikan dalam format ApiResponse yang konsisten.
    /// </remarks>
    [ApiController]
    [Route("api/wishlist")]
    [Authorize]
    [Produces("application/json")]
    public class WishlistController : BaseController
    {
        private readonly GetWishlistHandler _getWishlistHandler;
        private readonly AddToWishlistHandler _addToWishlistHandler;
        private readonly RemoveFromWishlistHandler _removeFromWishlistHandler;

        /// <summary>
        /// Inisialisasi WishlistController dengan seluruh handler wishlist.
        /// </summary>
        /// <param name="getWishlistHandler">Handler untuk mengambil daftar produk di wishlist.</param>
        /// <param name="addToWishlistHandler">Handler untuk menambahkan produk ke wishlist.</param>
        /// <param name="removeFromWishlistHandler">Handler untuk menghapus produk dari wishlist.</param>
        public WishlistController(
            GetWishlistHandler getWishlistHandler,
            AddToWishlistHandler addToWishlistHandler,
            RemoveFromWishlistHandler removeFromWishlistHandler)
        {
            _getWishlistHandler = getWishlistHandler;
            _addToWishlistHandler = addToWishlistHandler;
            _removeFromWishlistHandler = removeFromWishlistHandler;
        }

        /// <summary>
        /// Mengambil semua produk di wishlist user yang sedang login.
        /// </summary>
        /// <remarks>
        /// Endpoint: GET api/wishlist
        ///
        /// Flow get wishlist:
        /// 1. Endpoint membutuhkan autentikasi.
        /// 2. UserId diambil dari token current user secara otomatis di handler.
        /// 3. Jika gagal, kembalikan HTTP 400.
        /// 4. Jika berhasil, mapping DTO wishlist item ke WishlistItemResponse.
        /// </remarks>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse berisi daftar produk yang ada di wishlist user.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<WishlistItemResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(GetWishlistSuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedExample))]
        public async Task<IActionResult> GetWishlist(CancellationToken cancellationToken = default)
        {
            // Step 1: Eksekusi query handler untuk mengambil wishlist.
            var getWishlistResult = await _getWishlistHandler.Handle(new GetWishlistQuery(), cancellationToken);

            // Step 2: Jika gagal, kirim response error.
            if (!getWishlistResult.IsSuccess)
                return BadRequestResponse(getWishlistResult.Error ?? "Failed to get wishlist");

            // Step 3: Mapping DTO wishlist item ke response API.
            var wishlistItemResponses = getWishlistResult.Value!
                .Select(wishlistItemDto => new WishlistItemResponse(
                    wishlistItemDto.Id,
                    wishlistItemDto.ProductId,
                    wishlistItemDto.ProductName,
                    wishlistItemDto.ProductSlug,
                    wishlistItemDto.BasePrice,
                    wishlistItemDto.PrimaryImageUrl,
                    wishlistItemDto.IsActive,
                    wishlistItemDto.AddedAt
                ))
                .ToList();

            // Step 4: Kembalikan response sukses.
            return SuccessResponse(wishlistItemResponses, "Success get wishlist");
        }

        /// <summary>
        /// Menambahkan produk ke wishlist.
        /// </summary>
        /// <remarks>
        /// Endpoint: POST api/wishlist
        ///
        /// Flow add to wishlist:
        /// 1. Endpoint membutuhkan autentikasi.
        /// 2. Terima ProductId dari request body.
        /// 3. Mapping payload ke AddToWishlistCommand.
        /// 4. Jika produk tidak ditemukan atau sudah ada di wishlist, kembalikan HTTP 400.
        /// 5. Jika berhasil, kembalikan ID wishlist item yang baru dibuat.
        /// </remarks>
        /// <param name="addToWishlistRequest">Payload berisi ProductId yang akan ditambahkan ke wishlist.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse berisi ID wishlist item yang berhasil ditambahkan.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [SwaggerRequestExample(typeof(AddToWishlistRequest), typeof(AddToWishlistRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(AddToWishlistSuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(AddToWishlistBadRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedExample))]
        public async Task<IActionResult> AddToWishlist(
            [FromBody] AddToWishlistRequest addToWishlistRequest,
            CancellationToken cancellationToken = default)
        {
            // Step 1: Mapping request ke command aplikasi.
            var addToWishlistCommand = new AddToWishlistCommand(addToWishlistRequest.ProductId);

            // Step 2: Eksekusi proses add to wishlist.
            var addToWishlistResult = await _addToWishlistHandler.Handle(addToWishlistCommand, cancellationToken);

            // Step 3: Jika gagal, kirim response error.
            if (!addToWishlistResult.IsSuccess)
                return BadRequestResponse(addToWishlistResult.Error ?? "Failed to add to wishlist");

            // Step 4: Kembalikan ID wishlist item yang berhasil ditambahkan.
            return SuccessResponse(new { wishlistItemId = addToWishlistResult.Value }, "Product added to wishlist");
        }

        /// <summary>
        /// Menghapus produk dari wishlist.
        /// </summary>
        /// <remarks>
        /// Endpoint: DELETE api/wishlist/{productId}
        ///
        /// Flow remove from wishlist:
        /// 1. Endpoint membutuhkan autentikasi.
        /// 2. Terima ProductId dari route parameter.
        /// 3. Jika produk tidak ada di wishlist, kembalikan HTTP 404.
        /// 4. Jika validasi gagal, kembalikan HTTP 400.
        /// 5. Jika berhasil, kembalikan ID wishlist item yang dihapus.
        /// </remarks>
        /// <param name="wishlistItemId">ID wishlist item yang akan dihapus.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse berisi ID wishlist item yang berhasil dihapus.</returns>
        [HttpDelete("{wishlistItemId}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(RemoveFromWishlistSuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedExample))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(WishlistItemNotFoundExample))]
        public async Task<IActionResult> RemoveFromWishlist(
            Guid wishlistItemId,
            CancellationToken cancellationToken = default)
        {
            // Step 1: Bentuk command berdasarkan WishlistItemId dari route.
            var removeFromWishlistCommand = new RemoveFromWishlistCommand(wishlistItemId);

            // Step 2: Eksekusi proses remove from wishlist.
            var removeFromWishlistResult = await _removeFromWishlistHandler.Handle(removeFromWishlistCommand, cancellationToken);

            // Step 3: Jika gagal, petakan ke 404 (not found) atau 400 (bad request).
            if (!removeFromWishlistResult.IsSuccess)
            {
                if (removeFromWishlistResult.Error == "Wishlist item not found")
                    return NotFoundResponse(removeFromWishlistResult.Error);

                return BadRequestResponse(removeFromWishlistResult.Error ?? "Failed to remove from wishlist");
            }

            // Step 4: Kembalikan ID wishlist item yang berhasil dihapus.
            return SuccessResponse(new { wishlistItemId = removeFromWishlistResult.Value }, "Product removed from wishlist");
        }
    }
}

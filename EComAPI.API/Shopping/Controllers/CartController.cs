using EComAPI.API.Common;
using EComAPI.API.Shopping.Dtos.Requests;
using EComAPI.API.Shopping.Dtos.Responses;
using EComAPI.API.Shopping.Swagger.Examples;
using EComAPI.API.Shopping.Swagger.Examples.CartExample;
using EComAPI.API.Shopping.Swagger.Examples.CartExample.GetCart.Response;
using EComAPI.API.Shopping.Swagger.Examples.CartExample.AddToCart.Request;
using EComAPI.API.Shopping.Swagger.Examples.CartExample.AddToCart.Response;
using EComAPI.API.Shopping.Swagger.Examples.CartExample.UpdateCartItem.Request;
using EComAPI.API.Shopping.Swagger.Examples.CartExample.UpdateCartItem.Response;
using EComAPI.API.Shopping.Swagger.Examples.CartExample.RemoveFromCart.Response;
using EComAPI.API.Shopping.Swagger.Examples.CartExample.ClearCart.Response;
using EComAPI.Application.Shopping.Commands.CartCommands.AddToCart;
using EComAPI.Application.Shopping.Commands.CartCommands.ClearCart;
using EComAPI.Application.Shopping.Commands.CartCommands.RemoveFromCart;
using EComAPI.Application.Shopping.Commands.CartCommands.UpdateCartItem;
using EComAPI.Application.Shopping.DTOs;
using EComAPI.Application.Shopping.Queries.GetCart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Shopping.Controllers
{
    /// <summary>
    /// Endpoint untuk mengelola shopping cart user.
    /// </summary>
    /// <remarks>
    /// Alur umum (high-level flow):
    /// 1. User menambahkan, mengubah, atau menghapus item di cart.
    /// 2. Semua endpoint membutuhkan autentikasi.
    /// 3. Controller memetakan request ke command/query Application Layer.
    /// 4. Hasil dikembalikan dalam format ApiResponse yang konsisten.
    /// </remarks>
    [ApiController]
    [Route("api/cart")]
    [Authorize]
    [Produces("application/json")]
    public class CartController : BaseController
    {
        private readonly GetCartHandler _getCartHandler;
        private readonly AddToCartHandler _addToCartHandler;
        private readonly UpdateCartItemHandler _updateCartItemHandler;
        private readonly RemoveFromCartHandler _removeFromCartHandler;
        private readonly ClearCartHandler _clearCartHandler;

        /// <summary>
        /// Inisialisasi CartController dengan seluruh handler cart.
        /// </summary>
        /// <param name="getCartHandler">Handler untuk mengambil isi cart user.</param>
        /// <param name="addToCartHandler">Handler untuk menambahkan item ke cart.</param>
        /// <param name="updateCartItemHandler">Handler untuk mengubah quantity item di cart.</param>
        /// <param name="removeFromCartHandler">Handler untuk menghapus item dari cart.</param>
        /// <param name="clearCartHandler">Handler untuk mengosongkan seluruh isi cart.</param>
        public CartController(
            GetCartHandler getCartHandler,
            AddToCartHandler addToCartHandler,
            UpdateCartItemHandler updateCartItemHandler,
            RemoveFromCartHandler removeFromCartHandler,
            ClearCartHandler clearCartHandler)
        {
            _getCartHandler = getCartHandler;
            _addToCartHandler = addToCartHandler;
            _updateCartItemHandler = updateCartItemHandler;
            _removeFromCartHandler = removeFromCartHandler;
            _clearCartHandler = clearCartHandler;
        }

        /// <summary>
        /// Mengambil isi cart user yang sedang login.
        /// </summary>
        /// <remarks>
        /// Endpoint: GET api/cart
        ///
        /// Flow get cart:
        /// 1. Endpoint membutuhkan autentikasi.
        /// 2. UserId diambil dari token current user secara otomatis di handler.
        /// 3. Jika gagal, kembalikan HTTP 400.
        /// 4. Jika berhasil, mapping DTO cart ke CartResponse beserta daftar item.
        /// </remarks>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse berisi detail cart beserta seluruh item dan total harga.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<CartResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(GetCartSuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedExample))]
        public async Task<IActionResult> GetCart(CancellationToken cancellationToken = default)
        {
            // Step 1: Eksekusi query handler untuk mengambil cart.
            var getCartResult = await _getCartHandler.Handle(new GetCartQuery(), cancellationToken);

            // Step 2: Jika gagal, kirim response error.
            if (!getCartResult.IsSuccess)
                return BadRequestResponse(getCartResult.Error ?? "Failed to get cart");

            // Step 3: Mapping DTO cart ke response API.
            var cartDto = getCartResult.Value!;
            var cartResponse = new CartResponse(
                cartDto.Id,
                cartDto.UserId,
                cartDto.Items.Select(cartItemDto => new CartItemResponse(
                    cartItemDto.Id,
                    cartItemDto.ProductVariantId,
                    cartItemDto.ProductName,
                    cartItemDto.VariantSku,
                    cartItemDto.VariantSize,
                    cartItemDto.VariantColor,
                    cartItemDto.UnitPrice,
                    cartItemDto.Quantity,
                    cartItemDto.Subtotal
                )).ToList(),
                cartDto.TotalPrice,
                cartDto.TotalItems
            );

            // Step 4: Kembalikan response sukses.
            return SuccessResponse(cartResponse, "Success get cart");
        }

        /// <summary>
        /// Menambahkan produk variant ke cart.
        /// </summary>
        /// <remarks>
        /// Endpoint: POST api/cart/items
        ///
        /// Flow add to cart:
        /// 1. Endpoint membutuhkan autentikasi.
        /// 2. Terima ProductVariantId dan Quantity dari request body.
        /// 3. Mapping payload ke AddToCartCommand.
        /// 4. Jika variant tidak ditemukan atau stok tidak cukup, kembalikan HTTP 400.
        /// 5. Jika berhasil, kembalikan ID cart item yang baru dibuat.
        /// </remarks>
        /// <param name="addToCartRequest">Payload berisi ProductVariantId dan jumlah yang ditambahkan.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse berisi ID cart item yang berhasil ditambahkan.</returns>
        [HttpPost("items")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [SwaggerRequestExample(typeof(AddToCartRequest), typeof(AddToCartRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(AddToCartSuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(AddToCartBadRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedExample))]
        public async Task<IActionResult> AddItem(
            [FromBody] AddToCartRequest addToCartRequest,
            CancellationToken cancellationToken = default)
        {
            // Step 1: Mapping request ke command aplikasi.
            var addToCartCommand = new AddToCartCommand(addToCartRequest.ProductVariantId, addToCartRequest.Quantity);

            // Step 2: Eksekusi proses add to cart.
            var addToCartResult = await _addToCartHandler.Handle(addToCartCommand, cancellationToken);

            // Step 3: Jika gagal, kirim response error.
            if (!addToCartResult.IsSuccess)
                return BadRequestResponse(addToCartResult.Error ?? "Failed to add item to cart");

            // Step 4: Kembalikan ID cart item yang berhasil ditambahkan.
            return SuccessResponse(new { cartItemId = addToCartResult.Value }, "Item added to cart");
        }

        /// <summary>
        /// Mengubah quantity item di cart.
        /// </summary>
        /// <remarks>
        /// Endpoint: PATCH api/cart/items/{id}
        ///
        /// Flow update cart item:
        /// 1. Endpoint membutuhkan autentikasi.
        /// 2. Terima ID cart item dari route dan Quantity baru dari request body.
        /// 3. Jika cart item tidak ditemukan, kembalikan HTTP 404.
        /// 4. Jika validasi gagal, kembalikan HTTP 400.
        /// 5. Jika berhasil, kembalikan ID cart item yang diperbarui.
        /// </remarks>
        /// <param name="id">ID cart item yang akan diperbarui quantitynya.</param>
        /// <param name="updateCartItemRequest">Payload berisi quantity baru.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse berisi ID cart item yang berhasil diperbarui.</returns>
        [HttpPatch("items/{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [SwaggerRequestExample(typeof(UpdateCartItemRequest), typeof(UpdateCartItemRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(UpdateCartItemSuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(UpdateCartItemBadRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedExample))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(CartItemNotFoundExample))]
        public async Task<IActionResult> UpdateItem(
            Guid id,
            [FromBody] UpdateCartItemRequest updateCartItemRequest,
            CancellationToken cancellationToken = default)
        {
            // Step 1: Mapping route + request ke command aplikasi.
            var updateCartItemCommand = new UpdateCartItemCommand(id, updateCartItemRequest.Quantity);

            // Step 2: Eksekusi proses update cart item.
            var updateCartItemResult = await _updateCartItemHandler.Handle(updateCartItemCommand, cancellationToken);

            // Step 3: Jika gagal, petakan ke 404 (not found) atau 400 (bad request).
            if (!updateCartItemResult.IsSuccess)
            {
                if (updateCartItemResult.Error == "Cart item not found")
                    return NotFoundResponse(updateCartItemResult.Error);

                return BadRequestResponse(updateCartItemResult.Error ?? "Failed to update cart item");
            }

            // Step 4: Kembalikan ID cart item yang berhasil diperbarui.
            return SuccessResponse(new { cartItemId = updateCartItemResult.Value }, "Cart item updated");
        }

        /// <summary>
        /// Menghapus item dari cart.
        /// </summary>
        /// <remarks>
        /// Endpoint: DELETE api/cart/items/{id}
        ///
        /// Flow remove from cart:
        /// 1. Endpoint membutuhkan autentikasi.
        /// 2. Terima ID cart item dari route parameter.
        /// 3. Jika cart item tidak ditemukan, kembalikan HTTP 404.
        /// 4. Jika validasi gagal, kembalikan HTTP 400.
        /// 5. Jika berhasil, kembalikan ID cart item yang dihapus.
        /// </remarks>
        /// <param name="id">ID cart item yang akan dihapus.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse berisi ID cart item yang berhasil dihapus.</returns>
        [HttpDelete("items/{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(RemoveFromCartSuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedExample))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(CartItemNotFoundExample))]
        public async Task<IActionResult> RemoveItem(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            // Step 1: Bentuk command berdasarkan ID cart item dari route.
            var removeFromCartCommand = new RemoveFromCartCommand(id);

            // Step 2: Eksekusi proses remove from cart.
            var removeFromCartResult = await _removeFromCartHandler.Handle(removeFromCartCommand, cancellationToken);

            // Step 3: Jika gagal, petakan ke 404 (not found) atau 400 (bad request).
            if (!removeFromCartResult.IsSuccess)
            {
                if (removeFromCartResult.Error == "Cart item not found")
                    return NotFoundResponse(removeFromCartResult.Error);

                return BadRequestResponse(removeFromCartResult.Error ?? "Failed to remove item from cart");
            }

            // Step 4: Kembalikan ID cart item yang berhasil dihapus.
            return SuccessResponse(new { cartItemId = removeFromCartResult.Value }, "Item removed from cart");
        }

        /// <summary>
        /// Mengosongkan semua item di cart.
        /// </summary>
        /// <remarks>
        /// Endpoint: DELETE api/cart
        ///
        /// Flow clear cart:
        /// 1. Endpoint membutuhkan autentikasi.
        /// 2. UserId diambil dari token current user secara otomatis di handler.
        /// 3. Jika gagal, kembalikan HTTP 400.
        /// 4. Jika berhasil, kembalikan ID cart yang dikosongkan.
        /// </remarks>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse berisi ID cart yang berhasil dikosongkan.</returns>
        [HttpDelete]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ClearCartSuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedExample))]
        public async Task<IActionResult> ClearCart(CancellationToken cancellationToken = default)
        {
            // Step 1: Eksekusi proses clear cart.
            var clearCartResult = await _clearCartHandler.Handle(new ClearCartCommand(), cancellationToken);

            // Step 2: Jika gagal, kirim response error.
            if (!clearCartResult.IsSuccess)
                return BadRequestResponse(clearCartResult.Error ?? "Failed to clear cart");

            // Step 3: Kembalikan ID cart yang berhasil dikosongkan.
            return SuccessResponse(new { cartId = clearCartResult.Value }, "Cart cleared");
        }
    }
}

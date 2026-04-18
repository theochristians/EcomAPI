using EComAPI.API.Common;
using EComAPI.API.Authorization;
using EComAPI.API.Transaction.Dtos.Requests;
using EComAPI.API.Transaction.Dtos.Responses;
using EComAPI.API.Transaction.Swagger.Examples;
using EComAPI.API.Transaction.Swagger.Examples.OrderExample.CreateOrder.Request;
using EComAPI.API.Transaction.Swagger.Examples.OrderExample.CreateOrder.Response;
using EComAPI.API.Transaction.Swagger.Examples.OrderExample.GetOrder.Response;
using EComAPI.API.Transaction.Swagger.Examples.OrderExample.SubmitPaymentProof.Request;
using EComAPI.Application.Common.Authorization;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Transaction.Commands.OrderCommands.CreateOrder;
using EComAPI.Application.Transaction.Commands.OrderCommands.UpdateOrderStatus;
using EComAPI.Application.Transaction.Commands.PaymentCommands.ConfirmPayment;
using EComAPI.Application.Transaction.Commands.PaymentCommands.SubmitPaymentProof;
using EComAPI.Application.Transaction.DTOs;
using EComAPI.Application.Transaction.Queries.OrderQueries.GetOrderById;
using EComAPI.Application.Transaction.Queries.OrderQueries.GetOrdersByUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Transaction.Controllers
{
    /// <summary>
    /// Endpoint untuk mengelola order dan pembayaran.
    /// </summary>
    /// <remarks>
    /// Alur umum (high-level flow):
    /// 1. Customer membuat order lalu melakukan upload bukti pembayaran.
    /// 2. Admin mengkonfirmasi pembayaran dan memperbarui status pengiriman.
    /// 3. Controller memetakan request ke command/query Application Layer.
    /// 4. Hasil dikembalikan dalam format ApiResponse yang konsisten.
    /// </remarks>
    [ApiController]
    [Route("api/orders")]
    [Authorize]
    [Produces("application/json")]
    public class OrderController : BaseController
    {
        private readonly CreateOrderHandler _createOrderHandler;
        private readonly UpdateOrderStatusHandler _updateOrderStatusHandler;
        private readonly SubmitPaymentProofHandler _submitPaymentProofHandler;
        private readonly ConfirmPaymentHandler _confirmPaymentHandler;
        private readonly GetOrderByIdHandler _getOrderByIdHandler;
        private readonly GetOrdersByUserHandler _getOrdersByUserHandler;
        private readonly ICurrentUser _currentUser;

        /// <summary>
        /// Inisialisasi OrderController dengan seluruh handler order dan pembayaran.
        /// </summary>
        /// <param name="createOrderHandler">Handler untuk membuat order baru.</param>
        /// <param name="updateOrderStatusHandler">Handler untuk memperbarui status order.</param>
        /// <param name="submitPaymentProofHandler">Handler untuk upload bukti pembayaran.</param>
        /// <param name="confirmPaymentHandler">Handler untuk konfirmasi pembayaran oleh admin.</param>
        /// <param name="getOrderByIdHandler">Handler untuk mengambil detail order berdasarkan ID.</param>
        /// <param name="getOrdersByUserHandler">Handler untuk mengambil daftar order milik user.</param>
        /// <param name="currentUser">Service untuk mendapatkan informasi user yang sedang login.</param>
        public OrderController(
            CreateOrderHandler createOrderHandler,
            UpdateOrderStatusHandler updateOrderStatusHandler,
            SubmitPaymentProofHandler submitPaymentProofHandler,
            ConfirmPaymentHandler confirmPaymentHandler,
            GetOrderByIdHandler getOrderByIdHandler,
            GetOrdersByUserHandler getOrdersByUserHandler,
            ICurrentUser currentUser)
        {
            _createOrderHandler = createOrderHandler;
            _updateOrderStatusHandler = updateOrderStatusHandler;
            _submitPaymentProofHandler = submitPaymentProofHandler;
            _confirmPaymentHandler = confirmPaymentHandler;
            _getOrderByIdHandler = getOrderByIdHandler;
            _getOrdersByUserHandler = getOrdersByUserHandler;
            _currentUser = currentUser;
        }

        /// <summary>
        /// Membuat order baru. Customer only.
        /// </summary>
        /// <remarks>
        /// Endpoint: POST api/orders
        ///
        /// Flow create order:
        /// 1. Endpoint membutuhkan autentikasi (customer).
        /// 2. Terima payload order dari request body, termasuk daftar item dan data pengiriman.
        /// 3. Mapping payload ke CreateOrderCommand beserta UserId dari current user.
        /// 4. Jika gagal validasi/proses (misal stok habis, kupon tidak valid), kembalikan HTTP 400.
        /// 5. Jika berhasil, kembalikan nomor order yang dibuat.
        /// </remarks>
        /// <param name="createOrderRequest">Payload pembuatan order termasuk item dan alamat pengiriman.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse berisi nomor order yang berhasil dibuat.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [SwaggerRequestExample(typeof(CreateOrderRequest), typeof(CreateOrderRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(CreateOrderSuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(TransactionUnauthorizedExample))]
        public async Task<IActionResult> CreateOrder(
            [FromBody] CreateOrderRequest createOrderRequest,
            CancellationToken cancellationToken = default)
        {
            // Step 1: Mapping request ke command aplikasi beserta UserId dari current user.
            var createOrderCommand = new CreateOrderCommand(
                UserId: _currentUser.UserId,
                AddressId: createOrderRequest.AddressId,
                ShippingType: createOrderRequest.ShippingType,
                Items: createOrderRequest.Items.Select(i => new CreateOrderItemDto(
                    i.ProductVariantId,
                    i.Quantity)).ToList(),
                CouponCode: createOrderRequest.CouponCode,
                CustomerNote: createOrderRequest.CustomerNote);

            // Step 2: Eksekusi proses create order.
            var createOrderResult = await _createOrderHandler.Handle(createOrderCommand, cancellationToken);

            // Step 3: Jika gagal, kirim response error.
            if (!createOrderResult.IsSuccess)
                return BadRequestResponse(createOrderResult.Error ?? "Failed to create order");

            // Step 4: Kembalikan nomor order yang berhasil dibuat.
            return SuccessResponse(new { orderNumber = createOrderResult.Value }, "Order created successfully");
        }

        /// <summary>
        /// Mengambil daftar order milik user yang sedang login.
        /// </summary>
        /// <remarks>
        /// Endpoint: GET api/orders
        ///
        /// Flow get my orders:
        /// 1. Endpoint membutuhkan autentikasi (customer).
        /// 2. UserId diambil dari token current user secara otomatis.
        /// 3. Mendukung pagination melalui query parameter page dan pageSize.
        /// 4. Jika gagal, kembalikan HTTP 400.
        /// 5. Jika berhasil, kembalikan daftar order beserta total count.
        /// </remarks>
        /// <param name="page">Nomor halaman pagination, default 1.</param>
        /// <param name="pageSize">Jumlah item per halaman, default 10.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse berisi daftar order milik user beserta informasi pagination.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<OrderResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(TransactionUnauthorizedExample))]
        public async Task<IActionResult> GetMyOrders(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            // Step 1: Bentuk query berdasarkan UserId dari current user.
            var getOrdersByUserQuery = new GetOrdersByUserQuery(_currentUser.UserId, page, pageSize);

            // Step 2: Eksekusi query handler.
            var getOrdersByUserResult = await _getOrdersByUserHandler.Handle(getOrdersByUserQuery, cancellationToken);

            // Step 3: Jika gagal, kirim response error.
            if (!getOrdersByUserResult.IsSuccess)
                return BadRequestResponse(getOrdersByUserResult.Error ?? "Failed to get orders");

            // Step 4: Mapping DTO ke response API.
            var (orders, totalCount) = getOrdersByUserResult.Value;
            var orderResponses = orders.Select(orderDto => MapToOrderResponse(orderDto)).ToList();

            // Step 5: Kembalikan response sukses.
            return SuccessResponse(new { items = orderResponses, totalCount, page, pageSize }, "Success get orders");
        }

        /// <summary>
        /// Mengambil detail order berdasarkan ID.
        /// </summary>
        /// <remarks>
        /// Endpoint: GET api/orders/{id}
        ///
        /// Flow get order by ID:
        /// 1. Endpoint membutuhkan autentikasi.
        /// 2. Terima ID order dari route parameter.
        /// 3. Jika order tidak ditemukan atau gagal, kembalikan HTTP 400.
        /// 4. Jika berhasil, kembalikan detail order termasuk item dan informasi pembayaran.
        /// </remarks>
        /// <param name="id">ID order yang ingin diambil detailnya.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse berisi detail order lengkap beserta item dan pembayaran.</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<OrderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(GetOrderSuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(TransactionUnauthorizedExample))]
        public async Task<IActionResult> GetOrderById(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            // Step 1: Bentuk query berdasarkan ID order dari route.
            var getOrderByIdQuery = new GetOrderByIdQuery(id);

            // Step 2: Eksekusi query handler.
            var getOrderByIdResult = await _getOrderByIdHandler.Handle(getOrderByIdQuery, cancellationToken);

            // Step 3: Jika gagal, kirim response error.
            if (!getOrderByIdResult.IsSuccess)
                return BadRequestResponse(getOrderByIdResult.Error ?? "Failed to get order");

            // Step 4: Mapping DTO ke response API dan kembalikan.
            return SuccessResponse(MapToOrderResponse(getOrderByIdResult.Value!), "Success get order");
        }

        /// <summary>
        /// Upload bukti pembayaran. Customer only.
        /// </summary>
        /// <remarks>
        /// Endpoint: POST api/orders/{id}/payment/proof
        ///
        /// Flow submit payment proof:
        /// 1. Endpoint membutuhkan autentikasi (customer pemilik order).
        /// 2. Terima ID order dari route dan URL bukti pembayaran dari request body.
        /// 3. Handler memvalidasi bahwa order milik user yang sedang login.
        /// 4. Jika gagal validasi atau order tidak ditemukan, kembalikan HTTP 400.
        /// 5. Jika berhasil, kembalikan ID payment yang diperbarui.
        /// </remarks>
        /// <param name="id">ID order yang akan disubmit bukti pembayarannya.</param>
        /// <param name="submitPaymentProofRequest">Payload berisi URL bukti pembayaran dan metode pembayaran.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse berisi ID payment yang berhasil diperbarui.</returns>
        [HttpPost("{id:guid}/payment/proof")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [SwaggerRequestExample(typeof(SubmitPaymentProofRequest), typeof(SubmitPaymentProofRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(TransactionUnauthorizedExample))]
        public async Task<IActionResult> SubmitPaymentProof(
            Guid id,
            [FromBody] SubmitPaymentProofRequest submitPaymentProofRequest,
            CancellationToken cancellationToken = default)
        {
            // Step 1: Mapping route + request ke command aplikasi.
            var submitPaymentProofCommand = new SubmitPaymentProofCommand(
                OrderId: id,
                ProofImageUrl: submitPaymentProofRequest.ProofImageUrl,
                PaymentMethod: submitPaymentProofRequest.PaymentMethod);

            // Step 2: Eksekusi proses submit bukti pembayaran.
            var submitPaymentProofResult = await _submitPaymentProofHandler.Handle(
                submitPaymentProofCommand, cancellationToken);

            // Step 3: Jika gagal, kirim response error.
            if (!submitPaymentProofResult.IsSuccess)
                return BadRequestResponse(submitPaymentProofResult.Error ?? "Failed to submit payment proof");

            // Step 4: Kembalikan ID payment yang berhasil diperbarui.
            return SuccessResponse(new { paymentId = submitPaymentProofResult.Value }, "Payment proof submitted");
        }

        /// <summary>
        /// Update status order. Admin only.
        /// </summary>
        /// <remarks>
        /// Endpoint: PATCH api/orders/{id}/status
        ///
        /// Flow update order status:
        /// 1. Endpoint membutuhkan autentikasi (admin).
        /// 2. Terima ID order dari route dan status baru dari request body.
        /// 3. Untuk status shipped, field Courier dan TrackingNumber wajib diisi.
        /// 4. Mapping ke UpdateOrderStatusCommand dan eksekusi handler.
        /// 5. Jika gagal validasi atau transisi status tidak valid, kembalikan HTTP 400.
        /// 6. Jika berhasil, kembalikan ID order yang diperbarui.
        /// </remarks>
        /// <param name="id">ID order yang akan diperbarui statusnya.</param>
        /// <param name="updateOrderStatusRequest">Payload berisi status baru dan informasi pengiriman (opsional).</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse berisi ID order yang berhasil diperbarui statusnya.</returns>
        [HttpPatch("{id:guid}/status")]
        [HasPermission(Permissions.Orders.UpdateAny)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(TransactionUnauthorizedExample))]
        public async Task<IActionResult> UpdateOrderStatus(
            Guid id,
            [FromBody] UpdateOrderStatusRequest updateOrderStatusRequest,
            CancellationToken cancellationToken = default)
        {
            // Step 1: Mapping route + request ke command aplikasi.
            var updateOrderStatusCommand = new UpdateOrderStatusCommand(
                OrderId: id,
                NewStatus: updateOrderStatusRequest.Status,
                Courier: updateOrderStatusRequest.Courier,
                TrackingNumber: updateOrderStatusRequest.TrackingNumber,
                AdminNote: updateOrderStatusRequest.AdminNote);

            // Step 2: Eksekusi proses update status order.
            var updateOrderStatusResult = await _updateOrderStatusHandler.Handle(
                updateOrderStatusCommand, cancellationToken);

            // Step 3: Jika gagal, kirim response error.
            if (!updateOrderStatusResult.IsSuccess)
                return BadRequestResponse(updateOrderStatusResult.Error ?? "Failed to update order status");

            // Step 4: Kembalikan ID order yang berhasil diperbarui.
            return SuccessResponse(new { orderId = updateOrderStatusResult.Value }, "Order status updated");
        }

        /// <summary>
        /// Konfirmasi pembayaran order. Admin only.
        /// </summary>
        /// <remarks>
        /// Endpoint: POST api/orders/{id}/payment/confirm
        ///
        /// Flow confirm payment:
        /// 1. Endpoint membutuhkan autentikasi (admin).
        /// 2. Terima ID order dari route dan catatan admin dari request body.
        /// 3. Handler memvalidasi bahwa bukti pembayaran sudah diupload sebelumnya.
        /// 4. Konfirmasi payment lalu ubah status order menjadi paid.
        /// 5. Jika gagal validasi atau bukti belum diupload, kembalikan HTTP 400.
        /// 6. Jika berhasil, kembalikan ID payment yang dikonfirmasi.
        /// </remarks>
        /// <param name="id">ID order yang pembayarannya akan dikonfirmasi.</param>
        /// <param name="confirmPaymentRequest">Payload berisi catatan admin (opsional).</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse berisi ID payment yang berhasil dikonfirmasi.</returns>
        [HttpPost("{id:guid}/payment/confirm")]
        [HasPermission(Permissions.Orders.UpdateAny)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(TransactionUnauthorizedExample))]
        public async Task<IActionResult> ConfirmPayment(
            Guid id,
            [FromBody] ConfirmPaymentRequest confirmPaymentRequest,
            CancellationToken cancellationToken = default)
        {
            // Step 1: Mapping route + request ke command aplikasi.
            var confirmPaymentCommand = new ConfirmPaymentCommand(
                OrderId: id,
                AdminNote: confirmPaymentRequest.AdminNote);

            // Step 2: Eksekusi proses konfirmasi pembayaran.
            var confirmPaymentResult = await _confirmPaymentHandler.Handle(
                confirmPaymentCommand, cancellationToken);

            // Step 3: Jika gagal, kirim response error.
            if (!confirmPaymentResult.IsSuccess)
                return BadRequestResponse(confirmPaymentResult.Error ?? "Failed to confirm payment");

            // Step 4: Kembalikan ID payment yang berhasil dikonfirmasi.
            return SuccessResponse(new { paymentId = confirmPaymentResult.Value }, "Payment confirmed");
        }

        /// <summary>
        /// Memetakan OrderDto dari Application Layer ke OrderResponse untuk API response.
        /// </summary>
        /// <param name="orderDto">DTO order dari Application Layer beserta item dan pembayaran.</param>
        /// <returns>OrderResponse yang siap dikembalikan ke client.</returns>
        private static OrderResponse MapToOrderResponse(OrderDto orderDto)
        {
            return new OrderResponse(
                orderDto.Id,
                orderDto.OrderNumber,
                orderDto.Status,
                orderDto.ShippingRecipientName,
                orderDto.ShippingPhone,
                orderDto.ShippingFullAddress,
                orderDto.ShippingCity,
                orderDto.ShippingPostalCode,
                orderDto.TotalAmount,
                orderDto.ShippingCost,
                orderDto.DiscountAmount,
                orderDto.FinalAmount,
                orderDto.Courier,
                orderDto.TrackingNumber,
                orderDto.CustomerNote,
                orderDto.AdminNote,
                orderDto.CreatedAt,
                orderDto.Items.Select(orderItemDto => new OrderItemResponse(
                    orderItemDto.Id,
                    orderItemDto.ProductId,
                    orderItemDto.ProductVariantId,
                    orderItemDto.SnapshotProductName,
                    orderItemDto.SnapshotVariantName,
                    orderItemDto.SnapshotPrice,
                    orderItemDto.Quantity,
                    orderItemDto.SnapshotPrice * orderItemDto.Quantity)).ToList(),
                orderDto.Payment == null ? null : new PaymentResponse(
                    orderDto.Payment.Id,
                    orderDto.Payment.PaymentMethod,
                    orderDto.Payment.Amount,
                    orderDto.Payment.Status,
                    orderDto.Payment.ProofImageUrl,
                    orderDto.Payment.AdminNote,
                    orderDto.Payment.ConfirmedAt),
                orderDto.Reviews.Select(reviewDto => new ReviewResponse(
                    reviewDto.Id,
                    reviewDto.UserId,
                    reviewDto.OrderId,
                    reviewDto.ProductId,
                    reviewDto.Rating,
                    reviewDto.Comment,
                    reviewDto.CreatedAt,
                    reviewDto.Images.Select(reviewImageDto => new ReviewImageResponse(
                        reviewImageDto.Id,
                        reviewImageDto.ImageUrl,
                        reviewImageDto.DisplayOrder)).ToList())).ToList(),
                orderDto.Returns.Select(returnDto => new ReturnResponse(
                    returnDto.Id,
                    returnDto.OrderId,
                    returnDto.UserId,
                    returnDto.ReturnNumber,
                    returnDto.Reason,
                    returnDto.Status,
                    returnDto.RequestedAt,
                    returnDto.ApprovedAt,
                    returnDto.ApprovedBy,
                    returnDto.RefundAmount,
                    returnDto.BankName,
                    returnDto.BankAccountNumber,
                    returnDto.AccountHolderName,
                    returnDto.RefundDate,
                    returnDto.Items.Select(returnItemDto => new ReturnItemResponse(
                        returnItemDto.Id,
                        returnItemDto.OrderItemId,
                        returnItemDto.Quantity,
                        returnItemDto.Condition,
                        returnItemDto.AdminNote)).ToList(),
                    returnDto.Images.Select(returnImageDto => new ReturnImageResponse(
                        returnImageDto.Id,
                        returnImageDto.ImageUrl,
                        returnImageDto.Description,
                        returnImageDto.CreatedAt)).ToList())).ToList());
        }
    }
}

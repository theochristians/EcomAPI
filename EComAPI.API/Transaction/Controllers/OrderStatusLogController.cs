using EComAPI.API.Common;
using EComAPI.API.Transaction.Dtos.Responses;
using EComAPI.Application.Transaction.Queries.OrderStatusLogQueries.GetOrderStatusLogs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EComAPI.API.Transaction.Controllers
{
    /// <summary>
    /// Endpoint untuk melihat riwayat perubahan status order.
    /// </summary>
    /// <remarks>
    /// Alur umum (high-level flow):
    /// 1. Setiap perubahan status order dicatat sebagai log.
    /// 2. User yang terautentikasi dapat melihat riwayat status order.
    /// 3. Controller memetakan request ke query Application Layer.
    /// 4. Hasil dikembalikan dalam format ApiResponse yang konsisten.
    /// </remarks>
    [ApiController]
    [Route("api/orders/{orderId:guid}/status-logs")]
    [Authorize]
    [Produces("application/json")]
    public class OrderStatusLogController : BaseController
    {
        private readonly GetOrderStatusLogsHandler _getOrderStatusLogsHandler;

        /// <summary>
        /// Inisialisasi OrderStatusLogController dengan handler status log.
        /// </summary>
        /// <param name="getOrderStatusLogsHandler">Handler untuk mengambil riwayat perubahan status order.</param>
        public OrderStatusLogController(GetOrderStatusLogsHandler getOrderStatusLogsHandler)
        {
            _getOrderStatusLogsHandler = getOrderStatusLogsHandler;
        }

        /// <summary>
        /// Mengambil riwayat perubahan status untuk order tertentu.
        /// </summary>
        /// <remarks>
        /// Endpoint: GET api/orders/{orderId}/status-logs
        ///
        /// Flow get order status logs:
        /// 1. Endpoint membutuhkan autentikasi.
        /// 2. Terima orderId dari route parameter.
        /// 3. Mengambil seluruh log perubahan status untuk order tersebut.
        /// 4. Jika gagal, kembalikan HTTP 400.
        /// 5. Jika berhasil, kembalikan daftar log perubahan status.
        /// </remarks>
        /// <param name="orderId">ID order yang ingin dilihat riwayat statusnya.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse berisi daftar log perubahan status order.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<OrderStatusLogResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetOrderStatusLogs(
            Guid orderId,
            CancellationToken cancellationToken = default)
        {
            // Step 1: Bentuk query berdasarkan orderId dari route.
            var getOrderStatusLogsQuery = new GetOrderStatusLogsQuery(orderId);

            // Step 2: Eksekusi query handler.
            var getOrderStatusLogsResult = await _getOrderStatusLogsHandler
                .Handle(getOrderStatusLogsQuery, cancellationToken);

            // Step 3: Jika gagal, kirim response error.
            if (!getOrderStatusLogsResult.IsSuccess)
                return BadRequestResponse(getOrderStatusLogsResult.Error ?? "Failed to get order status logs");

            // Step 4: Mapping DTO ke response API.
            var statusLogResponses = getOrderStatusLogsResult.Value!
                .Select(statusLogDto => new OrderStatusLogResponse(
                    statusLogDto.Id,
                    statusLogDto.OrderId,
                    statusLogDto.Status,
                    statusLogDto.Note,
                    statusLogDto.ChangedAt,
                    statusLogDto.CreatedBy))
                .ToList();

            // Step 5: Kembalikan response sukses.
            return SuccessResponse(statusLogResponses, "Success get order status logs");
        }
    }
}

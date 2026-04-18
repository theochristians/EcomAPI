using EComAPI.API.Common;
using EComAPI.API.Authorization;
using EComAPI.API.Transaction.Dtos.Responses;
using EComAPI.Application.Common.Authorization;
using EComAPI.Application.Transaction.Queries.StockLogQueries.GetStockLogs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EComAPI.API.Transaction.Controllers
{
    /// <summary>
    /// Endpoint untuk melihat riwayat perubahan stok produk.
    /// </summary>
    /// <remarks>
    /// Alur umum (high-level flow):
    /// 1. Setiap perubahan stok varian produk dicatat sebagai log.
    /// 2. Admin dapat melihat riwayat perubahan stok per varian produk.
    /// 3. Controller memetakan request ke query Application Layer.
    /// 4. Hasil dikembalikan dalam format ApiResponse yang konsisten.
    /// </remarks>
    [ApiController]
    [Route("api/stock-logs")]
    [Authorize]
    [Produces("application/json")]
    public class StockLogController : BaseController
    {
        private readonly GetStockLogsHandler _getStockLogsHandler;

        /// <summary>
        /// Inisialisasi StockLogController dengan handler stock log.
        /// </summary>
        /// <param name="getStockLogsHandler">Handler untuk mengambil riwayat perubahan stok.</param>
        public StockLogController(GetStockLogsHandler getStockLogsHandler)
        {
            _getStockLogsHandler = getStockLogsHandler;
        }

        /// <summary>
        /// Mengambil riwayat perubahan stok untuk varian produk tertentu. Admin only.
        /// </summary>
        /// <remarks>
        /// Endpoint: GET api/stock-logs/variant/{variantId}
        ///
        /// Flow get stock logs:
        /// 1. Endpoint membutuhkan autentikasi (admin).
        /// 2. Terima variantId dari route parameter.
        /// 3. Mengambil seluruh log perubahan stok untuk varian tersebut.
        /// 4. Jika gagal, kembalikan HTTP 400.
        /// 5. Jika berhasil, kembalikan daftar log perubahan stok.
        /// </remarks>
        /// <param name="variantId">ID varian produk yang ingin dilihat riwayat stoknya.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse berisi daftar log perubahan stok.</returns>
        [HttpGet("variant/{variantId:guid}")]
        [HasPermission(Permissions.Orders.UpdateAny)]
        [ProducesResponseType(typeof(ApiResponse<List<StockLogResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetStockLogs(
            Guid variantId,
            CancellationToken cancellationToken = default)
        {
            // Step 1: Bentuk query berdasarkan variantId dari route.
            var getStockLogsQuery = new GetStockLogsQuery(variantId);

            // Step 2: Eksekusi query handler.
            var getStockLogsResult = await _getStockLogsHandler.Handle(getStockLogsQuery, cancellationToken);

            // Step 3: Jika gagal, kirim response error.
            if (!getStockLogsResult.IsSuccess)
                return BadRequestResponse(getStockLogsResult.Error ?? "Failed to get stock logs");

            // Step 4: Mapping DTO ke response API.
            var stockLogResponses = getStockLogsResult.Value!
                .Select(stockLogDto => new StockLogResponse(
                    stockLogDto.Id,
                    stockLogDto.ProductVariantId,
                    stockLogDto.Type,
                    stockLogDto.QuantityChange,
                    stockLogDto.StockBefore,
                    stockLogDto.StockAfter,
                    stockLogDto.ReferenceType,
                    stockLogDto.ReferenceId,
                    stockLogDto.Note,
                    stockLogDto.CreatedAt,
                    stockLogDto.CreatedBy))
                .ToList();

            // Step 5: Kembalikan response sukses.
            return SuccessResponse(stockLogResponses, "Success get stock logs");
        }
    }
}

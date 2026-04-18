using EComAPI.API.Common;
using EComAPI.API.Authorization;
using EComAPI.API.Transaction.Dtos.Requests;
using EComAPI.API.Transaction.Dtos.Responses;
using EComAPI.Application.Common.Authorization;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Transaction.Commands.ReturnCommands.ApproveReturn;
using EComAPI.Application.Transaction.Commands.ReturnCommands.CreateReturn;
using EComAPI.Application.Transaction.Commands.ReturnCommands.MarkReturnRefunded;
using EComAPI.Application.Transaction.Commands.ReturnCommands.RejectReturn;
using EComAPI.Application.Transaction.DTOs;
using EComAPI.Application.Transaction.Queries.ReturnQueries.GetReturnById;
using EComAPI.Application.Transaction.Queries.ReturnQueries.GetReturnsByUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EComAPI.API.Transaction.Controllers
{
    /// <summary>
    /// Endpoint untuk mengelola pengembalian (return) order.
    /// </summary>
    /// <remarks>
    /// Alur umum (high-level flow):
    /// 1. Customer mengajukan return untuk order yang sudah selesai.
    /// 2. Admin melakukan review dan approve/reject return request.
    /// 3. Setelah approve, admin menandai return sebagai refunded.
    /// 4. Controller memetakan request ke command/query Application Layer.
    /// 5. Hasil dikembalikan dalam format ApiResponse yang konsisten.
    /// </remarks>
    [ApiController]
    [Route("api/returns")]
    [Authorize]
    [Produces("application/json")]
    public class ReturnController : BaseController
    {
        private readonly CreateReturnHandler _createReturnHandler;
        private readonly ApproveReturnHandler _approveReturnHandler;
        private readonly RejectReturnHandler _rejectReturnHandler;
        private readonly MarkReturnRefundedHandler _markReturnRefundedHandler;
        private readonly GetReturnByIdHandler _getReturnByIdHandler;
        private readonly GetReturnsByUserHandler _getReturnsByUserHandler;
        private readonly ICurrentUser _currentUser;

        /// <summary>
        /// Inisialisasi ReturnController dengan seluruh handler return.
        /// </summary>
        /// <param name="createReturnHandler">Handler untuk membuat return request baru.</param>
        /// <param name="approveReturnHandler">Handler untuk approve return request.</param>
        /// <param name="rejectReturnHandler">Handler untuk reject return request.</param>
        /// <param name="markReturnRefundedHandler">Handler untuk menandai return sebagai refunded.</param>
        /// <param name="getReturnByIdHandler">Handler untuk mengambil detail return berdasarkan ID.</param>
        /// <param name="getReturnsByUserHandler">Handler untuk mengambil daftar return milik user.</param>
        /// <param name="currentUser">Service untuk mendapatkan informasi user yang sedang login.</param>
        public ReturnController(
            CreateReturnHandler createReturnHandler,
            ApproveReturnHandler approveReturnHandler,
            RejectReturnHandler rejectReturnHandler,
            MarkReturnRefundedHandler markReturnRefundedHandler,
            GetReturnByIdHandler getReturnByIdHandler,
            GetReturnsByUserHandler getReturnsByUserHandler,
            ICurrentUser currentUser)
        {
            _createReturnHandler = createReturnHandler;
            _approveReturnHandler = approveReturnHandler;
            _rejectReturnHandler = rejectReturnHandler;
            _markReturnRefundedHandler = markReturnRefundedHandler;
            _getReturnByIdHandler = getReturnByIdHandler;
            _getReturnsByUserHandler = getReturnsByUserHandler;
            _currentUser = currentUser;
        }

        /// <summary>
        /// Membuat return request untuk order yang sudah selesai. Customer only.
        /// </summary>
        /// <remarks>
        /// Endpoint: POST api/returns
        ///
        /// Flow create return:
        /// 1. Endpoint membutuhkan autentikasi (customer).
        /// 2. Terima payload return dari request body termasuk item, gambar bukti, dan info bank.
        /// 3. Mapping payload ke CreateReturnCommand.
        /// 4. Handler memvalidasi bahwa order sudah completed dan belum ada return aktif.
        /// 5. Jika gagal validasi/proses, kembalikan HTTP 400.
        /// 6. Jika berhasil, kembalikan ID return yang baru dibuat.
        /// </remarks>
        /// <param name="createReturnRequest">Payload pembuatan return termasuk orderId, alasan, item, gambar, dan info bank.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse berisi ID return yang berhasil dibuat.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateReturn(
            [FromBody] CreateReturnRequest createReturnRequest,
            CancellationToken cancellationToken = default)
        {
            // Step 1: Mapping request ke command aplikasi.
            var createReturnCommand = new CreateReturnCommand(
                OrderId: createReturnRequest.OrderId,
                Reason: createReturnRequest.Reason,
                Items: createReturnRequest.Items.Select(returnItemRequest => new CreateReturnItemDto(
                    returnItemRequest.OrderItemId, returnItemRequest.Quantity)).ToList(),
                Images: createReturnRequest.Images?.Select(returnImageRequest => new CreateReturnImageDto(
                    returnImageRequest.ImageUrl, returnImageRequest.Description)).ToList(),
                BankName: createReturnRequest.BankName,
                BankAccountNumber: createReturnRequest.BankAccountNumber,
                AccountHolderName: createReturnRequest.AccountHolderName);

            // Step 2: Eksekusi proses create return.
            var createReturnResult = await _createReturnHandler.Handle(createReturnCommand, cancellationToken);

            // Step 3: Jika gagal, kirim response error.
            if (!createReturnResult.IsSuccess)
                return BadRequestResponse(createReturnResult.Error ?? "Failed to create return request");

            // Step 4: Kembalikan ID return yang berhasil dibuat.
            return SuccessResponse(new { returnId = createReturnResult.Value }, "Return request created successfully");
        }

        /// <summary>
        /// Mengambil daftar return milik user yang sedang login (paginated).
        /// </summary>
        /// <remarks>
        /// Endpoint: GET api/returns
        ///
        /// Flow get my returns:
        /// 1. Endpoint membutuhkan autentikasi (customer).
        /// 2. UserId diambil dari token current user secara otomatis.
        /// 3. Mendukung pagination melalui query parameter page dan pageSize.
        /// 4. Jika gagal, kembalikan HTTP 400.
        /// 5. Jika berhasil, kembalikan daftar return beserta total count.
        /// </remarks>
        /// <param name="page">Nomor halaman pagination, default 1.</param>
        /// <param name="pageSize">Jumlah item per halaman, default 10.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>PaginatedApiResponse berisi daftar return milik user.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedApiResponse<List<ReturnResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyReturns(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            // Step 1: Bentuk query berdasarkan pagination.
            var getReturnsByUserQuery = new GetReturnsByUserQuery(page, pageSize);

            // Step 2: Eksekusi query handler.
            var getReturnsByUserResult = await _getReturnsByUserHandler
                .Handle(getReturnsByUserQuery, cancellationToken);

            // Step 3: Jika gagal, kirim response error.
            if (!getReturnsByUserResult.IsSuccess)
                return BadRequestResponse(getReturnsByUserResult.Error ?? "Failed to get returns");

            // Step 4: Mapping DTO ke response API.
            var (returns, totalCount) = getReturnsByUserResult.Value;
            var returnResponses = returns.Select(returnDto => MapToReturnResponse(returnDto)).ToList();

            // Step 5: Kembalikan response sukses dengan pagination.
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var queryString = Request.QueryString.ToString();

            return Ok(PaginatedApiResponse<List<ReturnResponse>>.Create(
                returnResponses, totalCount, page, pageSize, baseUrl, queryString, "Success get returns"));
        }

        /// <summary>
        /// Mengambil detail return berdasarkan ID.
        /// </summary>
        /// <remarks>
        /// Endpoint: GET api/returns/{id}
        ///
        /// Flow get return by ID:
        /// 1. Endpoint membutuhkan autentikasi.
        /// 2. Terima ID return dari route parameter.
        /// 3. Handler memvalidasi bahwa return milik user yang sedang login.
        /// 4. Jika return tidak ditemukan atau bukan milik user, kembalikan HTTP 400.
        /// 5. Jika berhasil, kembalikan detail return lengkap beserta item dan gambar.
        /// </remarks>
        /// <param name="id">ID return yang ingin diambil detailnya.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse berisi detail return lengkap.</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ReturnResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetReturnById(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            // Step 1: Bentuk query berdasarkan ID return dari route.
            var getReturnByIdQuery = new GetReturnByIdQuery(id);

            // Step 2: Eksekusi query handler.
            var getReturnByIdResult = await _getReturnByIdHandler.Handle(getReturnByIdQuery, cancellationToken);

            // Step 3: Jika gagal, kirim response error.
            if (!getReturnByIdResult.IsSuccess)
                return BadRequestResponse(getReturnByIdResult.Error ?? "Failed to get return");

            // Step 4: Mapping DTO ke response API dan kembalikan.
            return SuccessResponse(MapToReturnResponse(getReturnByIdResult.Value!), "Success get return");
        }

        /// <summary>
        /// Approve return request. Admin only.
        /// </summary>
        /// <remarks>
        /// Endpoint: POST api/returns/{id}/approve
        ///
        /// Flow approve return:
        /// 1. Endpoint membutuhkan autentikasi (admin).
        /// 2. Terima ID return dari route dan payload approve dari body.
        /// 3. Mapping ke ApproveReturnCommand termasuk jumlah refund dan kondisi item.
        /// 4. Jika gagal validasi/proses, kembalikan HTTP 400.
        /// 5. Jika berhasil, kembalikan ID return yang di-approve.
        /// </remarks>
        /// <param name="id">ID return yang akan di-approve.</param>
        /// <param name="approveReturnRequest">Payload approve berisi jumlah refund dan kondisi item (opsional).</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse berisi ID return yang berhasil di-approve.</returns>
        [HttpPost("{id:guid}/approve")]
        [HasPermission(Permissions.Orders.UpdateAny)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ApproveReturn(
            Guid id,
            [FromBody] ApproveReturnRequest approveReturnRequest,
            CancellationToken cancellationToken = default)
        {
            // Step 1: Mapping route + request ke command aplikasi.
            var approveReturnCommand = new ApproveReturnCommand(
                ReturnId: id,
                RefundAmount: approveReturnRequest.RefundAmount,
                ItemConditions: approveReturnRequest.ItemConditions?.Select(itemCondition => new ApproveReturnItemDto(
                    itemCondition.ReturnItemId, itemCondition.Condition, itemCondition.AdminNote)).ToList());

            // Step 2: Eksekusi proses approve return.
            var approveReturnResult = await _approveReturnHandler.Handle(approveReturnCommand, cancellationToken);

            // Step 3: Jika gagal, kirim response error.
            if (!approveReturnResult.IsSuccess)
                return BadRequestResponse(approveReturnResult.Error ?? "Failed to approve return");

            // Step 4: Kembalikan ID return yang berhasil di-approve.
            return SuccessResponse(new { returnId = id }, "Return approved successfully");
        }

        /// <summary>
        /// Reject return request. Admin only.
        /// </summary>
        /// <remarks>
        /// Endpoint: POST api/returns/{id}/reject
        ///
        /// Flow reject return:
        /// 1. Endpoint membutuhkan autentikasi (admin).
        /// 2. Terima ID return dari route.
        /// 3. Mapping ke RejectReturnCommand.
        /// 4. Jika gagal validasi/proses, kembalikan HTTP 400.
        /// 5. Jika berhasil, kembalikan ID return yang di-reject.
        /// </remarks>
        /// <param name="id">ID return yang akan di-reject.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse berisi ID return yang berhasil di-reject.</returns>
        [HttpPost("{id:guid}/reject")]
        [HasPermission(Permissions.Orders.UpdateAny)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RejectReturn(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            // Step 1: Bentuk command reject return.
            var rejectReturnCommand = new RejectReturnCommand(ReturnId: id);

            // Step 2: Eksekusi proses reject return.
            var rejectReturnResult = await _rejectReturnHandler.Handle(rejectReturnCommand, cancellationToken);

            // Step 3: Jika gagal, kirim response error.
            if (!rejectReturnResult.IsSuccess)
                return BadRequestResponse(rejectReturnResult.Error ?? "Failed to reject return");

            // Step 4: Kembalikan ID return yang berhasil di-reject.
            return SuccessResponse(new { returnId = id }, "Return rejected successfully");
        }

        /// <summary>
        /// Menandai return sebagai sudah di-refund. Admin only.
        /// </summary>
        /// <remarks>
        /// Endpoint: POST api/returns/{id}/refund
        ///
        /// Flow mark return refunded:
        /// 1. Endpoint membutuhkan autentikasi (admin).
        /// 2. Terima ID return dari route.
        /// 3. Mapping ke MarkReturnRefundedCommand.
        /// 4. Handler memvalidasi bahwa return sudah di-approve sebelumnya.
        /// 5. Jika gagal validasi/proses, kembalikan HTTP 400.
        /// 6. Jika berhasil, kembalikan ID return yang sudah di-refund.
        /// </remarks>
        /// <param name="id">ID return yang akan ditandai sebagai refunded.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse berisi ID return yang berhasil ditandai refunded.</returns>
        [HttpPost("{id:guid}/refund")]
        [HasPermission(Permissions.Orders.UpdateAny)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> MarkReturnRefunded(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            // Step 1: Bentuk command mark return refunded.
            var markReturnRefundedCommand = new MarkReturnRefundedCommand(ReturnId: id);

            // Step 2: Eksekusi proses mark return refunded.
            var markReturnRefundedResult = await _markReturnRefundedHandler
                .Handle(markReturnRefundedCommand, cancellationToken);

            // Step 3: Jika gagal, kirim response error.
            if (!markReturnRefundedResult.IsSuccess)
                return BadRequestResponse(markReturnRefundedResult.Error ?? "Failed to mark return as refunded");

            // Step 4: Kembalikan ID return yang berhasil ditandai refunded.
            return SuccessResponse(new { returnId = id }, "Return marked as refunded");
        }

        /// <summary>
        /// Memetakan ReturnDto dari Application Layer ke ReturnResponse untuk API response.
        /// </summary>
        /// <param name="returnDto">DTO return dari Application Layer beserta item dan gambar.</param>
        /// <returns>ReturnResponse yang siap dikembalikan ke client.</returns>
        private static ReturnResponse MapToReturnResponse(ReturnDto returnDto)
        {
            return new ReturnResponse(
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
                    returnImageDto.CreatedAt)).ToList());
        }
    }
}

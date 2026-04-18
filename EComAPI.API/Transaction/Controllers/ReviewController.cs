using EComAPI.API.Common;
using EComAPI.API.Transaction.Dtos.Requests;
using EComAPI.API.Transaction.Dtos.Responses;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Transaction.Commands.ReviewCommands.CreateReview;
using EComAPI.Application.Transaction.Commands.ReviewCommands.DeleteReview;
using EComAPI.Application.Transaction.Commands.ReviewCommands.UpdateReview;
using EComAPI.Application.Transaction.DTOs;
using EComAPI.Application.Transaction.Queries.ReviewQueries.GetReviewsByProduct;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EComAPI.API.Transaction.Controllers
{
    /// <summary>
    /// Endpoint untuk mengelola review produk.
    /// </summary>
    /// <remarks>
    /// Alur umum (high-level flow):
    /// 1. Customer yang sudah menyelesaikan order dapat membuat review.
    /// 2. Review produk dapat dilihat publik tanpa autentikasi.
    /// 3. Customer hanya dapat mengubah/menghapus review miliknya sendiri.
    /// 4. Controller memetakan request ke command/query Application Layer.
    /// 5. Hasil dikembalikan dalam format ApiResponse yang konsisten.
    /// </remarks>
    [ApiController]
    [Route("api/reviews")]
    [Produces("application/json")]
    public class ReviewController : BaseController
    {
        private readonly CreateReviewHandler _createReviewHandler;
        private readonly UpdateReviewHandler _updateReviewHandler;
        private readonly DeleteReviewHandler _deleteReviewHandler;
        private readonly GetReviewsByProductHandler _getReviewsByProductHandler;
        private readonly ICurrentUser _currentUser;

        /// <summary>
        /// Inisialisasi ReviewController dengan seluruh handler review.
        /// </summary>
        /// <param name="createReviewHandler">Handler untuk membuat review produk baru.</param>
        /// <param name="updateReviewHandler">Handler untuk memperbarui review.</param>
        /// <param name="deleteReviewHandler">Handler untuk menghapus review (soft delete).</param>
        /// <param name="getReviewsByProductHandler">Handler untuk mengambil daftar review berdasarkan produk.</param>
        /// <param name="currentUser">Service untuk mendapatkan informasi user yang sedang login.</param>
        public ReviewController(
            CreateReviewHandler createReviewHandler,
            UpdateReviewHandler updateReviewHandler,
            DeleteReviewHandler deleteReviewHandler,
            GetReviewsByProductHandler getReviewsByProductHandler,
            ICurrentUser currentUser)
        {
            _createReviewHandler = createReviewHandler;
            _updateReviewHandler = updateReviewHandler;
            _deleteReviewHandler = deleteReviewHandler;
            _getReviewsByProductHandler = getReviewsByProductHandler;
            _currentUser = currentUser;
        }

        /// <summary>
        /// Mengambil daftar review untuk produk tertentu (public).
        /// </summary>
        /// <remarks>
        /// Endpoint: GET api/reviews/product/{productId}
        ///
        /// Flow get reviews by product:
        /// 1. Endpoint dapat diakses tanpa autentikasi.
        /// 2. Mendukung pagination melalui query parameter page dan pageSize.
        /// 3. Review diurutkan dari yang terbaru.
        /// 4. Jika gagal, kembalikan HTTP 400.
        /// 5. Jika berhasil, kembalikan daftar review beserta total count.
        /// </remarks>
        /// <param name="productId">ID produk yang ingin dilihat reviewnya.</param>
        /// <param name="page">Nomor halaman pagination, default 1.</param>
        /// <param name="pageSize">Jumlah item per halaman, default 10.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>PaginatedApiResponse berisi daftar review produk.</returns>
        [HttpGet("product/{productId:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PaginatedApiResponse<List<ReviewResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetReviewsByProduct(
            Guid productId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            // Step 1: Bentuk query berdasarkan productId dan pagination.
            var getReviewsByProductQuery = new GetReviewsByProductQuery(productId, page, pageSize);

            // Step 2: Eksekusi query handler.
            var getReviewsByProductResult = await _getReviewsByProductHandler
                .Handle(getReviewsByProductQuery, cancellationToken);

            // Step 3: Jika gagal, kirim response error.
            if (!getReviewsByProductResult.IsSuccess)
                return BadRequestResponse(getReviewsByProductResult.Error ?? "Failed to get reviews");

            // Step 4: Mapping DTO ke response API.
            var (reviews, totalCount) = getReviewsByProductResult.Value;
            var reviewResponses = reviews.Select(reviewDto => MapToReviewResponse(reviewDto)).ToList();

            // Step 5: Kembalikan response sukses dengan pagination.
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var queryString = Request.QueryString.ToString();

            return Ok(PaginatedApiResponse<List<ReviewResponse>>.Create(
                reviewResponses, totalCount, page, pageSize, baseUrl, queryString, "Success get reviews"));
        }

        /// <summary>
        /// Membuat review untuk produk. Customer harus sudah menyelesaikan order produk tersebut.
        /// </summary>
        /// <remarks>
        /// Endpoint: POST api/reviews
        ///
        /// Flow create review:
        /// 1. Endpoint membutuhkan autentikasi (customer).
        /// 2. Terima payload review dari request body termasuk rating, komentar, dan gambar.
        /// 3. Mapping payload ke CreateReviewCommand.
        /// 4. Handler memvalidasi bahwa customer telah menyelesaikan order untuk produk tersebut.
        /// 5. Jika gagal validasi/proses, kembalikan HTTP 400.
        /// 6. Jika berhasil, kembalikan ID review yang baru dibuat.
        /// </remarks>
        /// <param name="createReviewRequest">Payload pembuatan review termasuk productId, rating, komentar, dan gambar.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse berisi ID review yang berhasil dibuat.</returns>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateReview(
            [FromBody] CreateReviewRequest createReviewRequest,
            CancellationToken cancellationToken = default)
        {
            // Step 1: Mapping request ke command aplikasi.
            var createReviewCommand = new CreateReviewCommand(
                OrderId: createReviewRequest.OrderId,
                ProductId: createReviewRequest.ProductId,
                Rating: createReviewRequest.Rating,
                Comment: createReviewRequest.Comment,
                Images: createReviewRequest.Images?.Select(reviewImageRequest => new CreateReviewImageDto(
                    reviewImageRequest.ImageUrl, reviewImageRequest.DisplayOrder)).ToList());

            // Step 2: Eksekusi proses create review.
            var createReviewResult = await _createReviewHandler.Handle(createReviewCommand, cancellationToken);

            // Step 3: Jika gagal, kirim response error.
            if (!createReviewResult.IsSuccess)
                return BadRequestResponse(createReviewResult.Error ?? "Failed to create review");

            // Step 4: Kembalikan ID review yang berhasil dibuat.
            return SuccessResponse(new { reviewId = createReviewResult.Value }, "Review created successfully");
        }

        /// <summary>
        /// Memperbarui review milik sendiri.
        /// </summary>
        /// <remarks>
        /// Endpoint: PUT api/reviews/{id}
        ///
        /// Flow update review:
        /// 1. Endpoint membutuhkan autentikasi (customer pemilik review).
        /// 2. Terima ID review dari route dan payload perubahan dari body.
        /// 3. Mapping ke UpdateReviewCommand.
        /// 4. Handler memvalidasi bahwa review milik user yang sedang login.
        /// 5. Jika gagal validasi/proses, kembalikan HTTP 400.
        /// 6. Jika berhasil, kembalikan ID review yang diperbarui.
        /// </remarks>
        /// <param name="id">ID review yang akan diperbarui.</param>
        /// <param name="updateReviewRequest">Payload perubahan review termasuk rating dan komentar.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse berisi ID review yang berhasil diperbarui.</returns>
        [HttpPut("{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateReview(
            Guid id,
            [FromBody] UpdateReviewRequest updateReviewRequest,
            CancellationToken cancellationToken = default)
        {
            // Step 1: Mapping route + request ke command aplikasi.
            var updateReviewCommand = new UpdateReviewCommand(
                ReviewId: id,
                Rating: updateReviewRequest.Rating,
                Comment: updateReviewRequest.Comment);

            // Step 2: Eksekusi proses update review.
            var updateReviewResult = await _updateReviewHandler.Handle(updateReviewCommand, cancellationToken);

            // Step 3: Jika gagal, kirim response error.
            if (!updateReviewResult.IsSuccess)
                return BadRequestResponse(updateReviewResult.Error ?? "Failed to update review");

            // Step 4: Kembalikan ID review yang berhasil diperbarui.
            return SuccessResponse(new { reviewId = updateReviewResult.Value }, "Review updated successfully");
        }

        /// <summary>
        /// Menghapus review milik sendiri (soft delete).
        /// </summary>
        /// <remarks>
        /// Endpoint: DELETE api/reviews/{id}
        ///
        /// Flow delete review:
        /// 1. Endpoint membutuhkan autentikasi (customer pemilik review).
        /// 2. Terima ID review dari route.
        /// 3. Mapping ke DeleteReviewCommand.
        /// 4. Handler memvalidasi bahwa review milik user yang sedang login.
        /// 5. Jika gagal validasi/proses, kembalikan HTTP 400.
        /// 6. Jika berhasil, kembalikan ID review yang dihapus.
        /// </remarks>
        /// <param name="id">ID review yang akan dihapus.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse berisi ID review yang berhasil dihapus.</returns>
        [HttpDelete("{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteReview(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            // Step 1: Bentuk command delete review.
            var deleteReviewCommand = new DeleteReviewCommand(ReviewId: id);

            // Step 2: Eksekusi proses delete review.
            var deleteReviewResult = await _deleteReviewHandler.Handle(deleteReviewCommand, cancellationToken);

            // Step 3: Jika gagal, kirim response error.
            if (!deleteReviewResult.IsSuccess)
                return BadRequestResponse(deleteReviewResult.Error ?? "Failed to delete review");

            // Step 4: Kembalikan ID review yang berhasil dihapus.
            return SuccessResponse(new { reviewId = deleteReviewResult.Value }, "Review deleted successfully");
        }

        /// <summary>
        /// Memetakan ReviewDto dari Application Layer ke ReviewResponse untuk API response.
        /// </summary>
        /// <param name="reviewDto">DTO review dari Application Layer beserta gambar.</param>
        /// <returns>ReviewResponse yang siap dikembalikan ke client.</returns>
        private static ReviewResponse MapToReviewResponse(ReviewDto reviewDto)
        {
            return new ReviewResponse(
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
                    reviewImageDto.DisplayOrder)).ToList());
        }
    }
}

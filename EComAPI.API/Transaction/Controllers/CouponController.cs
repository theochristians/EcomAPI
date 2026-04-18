using EComAPI.API.Common;
using EComAPI.API.Authorization;
using EComAPI.API.Transaction.Dtos.Requests;
using EComAPI.API.Transaction.Dtos.Responses;
using EComAPI.API.Transaction.Swagger.Examples;
using EComAPI.API.Transaction.Swagger.Examples.CouponExample.CreateCoupon.Request;
using EComAPI.Application.Common.Authorization;
using EComAPI.Application.Transaction.Commands.CouponCommands.CreateCoupon;
using EComAPI.Application.Transaction.Queries.CouponQueries.GetAllCoupons;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Transaction.Controllers
{
    /// <summary>
    /// Endpoint untuk mengelola kupon diskon.
    /// </summary>
    /// <remarks>
    /// Alur umum (high-level flow):
    /// 1. Admin membuat dan mengelola kupon diskon.
    /// 2. Customer menggunakan kode kupon saat checkout melalui CreateOrder.
    /// 3. Controller memetakan request ke command Application Layer.
    /// 4. Hasil dikembalikan dalam format ApiResponse yang konsisten.
    /// </remarks>
    [ApiController]
    [Route("api/coupons")]
    [Authorize]
    [Produces("application/json")]
    public class CouponController : BaseController
    {
        private readonly CreateCouponHandler _createCouponHandler;
        private readonly GetAllCouponsHandler _getAllCouponsHandler;

        /// <summary>
        /// Inisialisasi CouponController dengan seluruh handler kupon.
        /// </summary>
        /// <param name="createCouponHandler">Handler untuk membuat kupon diskon baru.</param>
        /// <param name="getAllCouponsHandler">Handler untuk mendapatkan semua kupon diskon.</param>
        public CouponController(
            CreateCouponHandler createCouponHandler,
            GetAllCouponsHandler getAllCouponsHandler)
        {
            _createCouponHandler = createCouponHandler;
            _getAllCouponsHandler = getAllCouponsHandler;
        }

        /// <summary>
        /// Membuat kupon diskon baru. Admin only.
        /// </summary>
        /// <remarks>
        /// Endpoint: POST api/coupons
        ///
        /// Flow create coupon:
        /// 1. Endpoint membutuhkan autentikasi (admin).
        /// 2. Terima payload kupon dari request body.
        /// 3. Mapping payload ke CreateCouponCommand.
        /// 4. Jika kode kupon sudah ada atau data tidak valid, kembalikan HTTP 400.
        /// 5. Jika berhasil, kembalikan ID kupon yang baru dibuat.
        /// </remarks>
        /// <param name="createCouponRequest">Payload pembuatan kupon termasuk kode, tipe diskon, dan masa berlaku.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse berisi ID kupon yang berhasil dibuat.</returns>
        [HttpPost]
        [HasPermission(Permissions.Orders.UpdateAny)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [SwaggerRequestExample(typeof(CreateCouponRequest), typeof(CreateCouponRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(TransactionUnauthorizedExample))]
        public async Task<IActionResult> CreateCoupon(
            [FromBody] CreateCouponRequest createCouponRequest,
            CancellationToken cancellationToken = default)
        {
            // Step 1: Mapping request ke command aplikasi.
            var createCouponCommand = new CreateCouponCommand(
                Code: createCouponRequest.Code,
                DiscountAmount: createCouponRequest.DiscountAmount,
                DiscountType: createCouponRequest.DiscountType,
                MaxUsage: createCouponRequest.MaxUsage,
                ValidFrom: createCouponRequest.ValidFrom,
                ValidUntil: createCouponRequest.ValidUntil,
                MinimumPurchase: createCouponRequest.MinimumPurchase,
                MaxDiscount: createCouponRequest.MaxDiscount);

            // Step 2: Eksekusi proses create kupon.
            var createCouponResult = await _createCouponHandler.Handle(createCouponCommand, cancellationToken);

            // Step 3: Jika gagal, kirim response error.
            if (!createCouponResult.IsSuccess)
                return BadRequestResponse(createCouponResult.Error ?? "Failed to create coupon");

            // Step 4: Kembalikan ID kupon yang berhasil dibuat.
            return SuccessResponse(new { couponId = createCouponResult.Value }, "Coupon created successfully");
        }

        /// <summary>
        /// Mendapatkan semua kupon diskon. User terautentikasi (admin/customer).
        /// </summary>
        /// <remarks>
        /// Endpoint: GET api/coupons
        ///
        /// Flow get all coupons:
        /// 1. Endpoint membutuhkan autentikasi.
        /// 2. Mengembalikan seluruh kupon yang tersedia diurutkan dari yang terbaru.
        /// </remarks>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse berisi list semua kupon.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<CouponResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAllCoupons(CancellationToken cancellationToken = default)
        {
            // Step 1: Eksekusi query mengambil semua kupon.
            var getAllCouponsResult = await _getAllCouponsHandler.Handle(
                new GetAllCouponsQuery(), cancellationToken);

            // Step 2: Jika gagal, kirim response error.
            if (!getAllCouponsResult.IsSuccess)
                return BadRequestResponse(getAllCouponsResult.Error ?? "Failed to retrieve coupons");

            // Step 3: Mapping ke response DTO.
            var couponResponses = getAllCouponsResult.Value!
                .Select(coupon => new CouponResponse(
                    coupon.Id,
                    coupon.Code,
                    coupon.DiscountAmount,
                    coupon.DiscountType,
                    coupon.MinimumPurchase,
                    coupon.MaxDiscount,
                    coupon.MaxUsage,
                    coupon.UsedCount,
                    coupon.ValidFrom,
                    coupon.ValidUntil,
                    coupon.IsActive));

            // Step 4: Kembalikan list kupon.
            return SuccessResponse(couponResponses, "Coupons retrieved successfully");
        }
    }
}

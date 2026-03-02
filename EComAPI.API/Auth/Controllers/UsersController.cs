using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Swashbuckle.AspNetCore.Filters;
using EComAPI.API.Common;
using EComAPI.API.Authorization;
using EComAPI.API.Auth.Dtos.Response;
using EComAPI.API.Auth.Swagger.Examples;
using EComAPI.API.Auth.Swagger.Examples.GetOwnProfile.Response;
using EComAPI.API.Auth.Swagger.Examples.UpdateOwnProfile.Request;
using EComAPI.API.Auth.Swagger.Examples.UpdateOwnProfile.Response;
using EComAPI.API.Auth.Swagger.Examples.DeleteOwnAccount.Response;
using EComAPI.Application.Common.Authorization;
using EComAPI.Application.Auth.Queries.GetOwnProfile;
using EComAPI.Application.Auth.Commands.UpdateOwnProfile;
using EComAPI.Application.Auth.Commands.DeleteOwnAccount;
using EComAPI.Application.Auth.Commands.SendEmailVerification;
using EComAPI.Application.Auth.Commands.VerifyEmail;
using EComAPI.API.Auth.Dtos.Request;

namespace EComAPI.API.Auth.Controllers
{
    /// <summary>
    /// Endpoint manajemen data profil pengguna yang sedang login.
    /// </summary>
    /// <remarks>
    /// Alur umum (high-level flow):
    /// 1. Request masuk ke endpoint yang membutuhkan permission.
    /// 2. Controller membentuk command/query ke Application Layer.
    /// 3. Handler menjalankan business logic sesuai use case.
    /// 4. Hasil dikembalikan sebagai ApiResponse yang konsisten.
    /// </remarks>
    [ApiController]
    [Route("api/users")]
    public class UsersController : BaseController
    {
        private readonly GetOwnProfileHandler _getOwnProfileHandler;
        private readonly UpdateOwnProfileHandler _updateOwnProfileHandler;
        private readonly DeleteOwnAccountHandler _deleteOwnAccountHandler;
        private readonly SendEmailVerificationHandler _sendEmailVerificationHandler;
        private readonly VerifyEmailHandler _verifyEmailHandler;

        /// <summary>
        /// Inisialisasi UsersController dengan handler untuk fitur profil pengguna.
        /// </summary>
        /// <param name="getOwnProfileHandler">Handler untuk mengambil profil user login.</param>
        /// <param name="updateOwnProfileHandler">Handler untuk memperbarui profil user login.</param>
        /// <param name="deleteOwnAccountHandler">Handler untuk menghapus akun milik user login.</param>
        /// <param name="sendEmailVerificationHandler">Handler untuk mengirim OTP verifikasi email.</param>
        /// <param name="verifyEmailHandler">Handler untuk verifikasi email menggunakan OTP.</param>
        public UsersController(
            GetOwnProfileHandler getOwnProfileHandler,
            UpdateOwnProfileHandler updateOwnProfileHandler,
            DeleteOwnAccountHandler deleteOwnAccountHandler,
            SendEmailVerificationHandler sendEmailVerificationHandler,
            VerifyEmailHandler verifyEmailHandler)
        {
            _getOwnProfileHandler = getOwnProfileHandler;
            _updateOwnProfileHandler = updateOwnProfileHandler;
            _deleteOwnAccountHandler = deleteOwnAccountHandler;
            _sendEmailVerificationHandler = sendEmailVerificationHandler;
            _verifyEmailHandler = verifyEmailHandler;
        }

        /// <summary>
        /// Mengambil data profil pengguna yang sedang login.
        /// </summary>
        /// <remarks>
        /// Endpoint: GET api/users/me
        ///
        /// Flow get own profile:
        /// 1. Endpoint memerlukan permission Users.ReadOwn.
        /// 2. Controller memanggil query handler tanpa payload tambahan.
        /// 3. Jika gagal, kembalikan HTTP 400.
        /// 4. Jika berhasil, mapping DTO ke UserProfileResponse.
        /// 5. Kembalikan data profil user yang sedang login.
        /// </remarks>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse berisi profil pengguna aktif.</returns>
        [HttpGet]
        [Route("me")]
        [HasPermission(Permissions.Users.ReadOwn)]
        [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(GetOwnProfileSuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedExample))]
        public async Task<IActionResult> GetOwnProfile(CancellationToken cancellationToken)
        {
            // Step 1: Eksekusi query untuk mengambil profil user login.
            var getOwnProfileResult = await _getOwnProfileHandler.Handle(cancellationToken);

            // Step 2: Jika gagal, kirim response error.
            if (!getOwnProfileResult.IsSuccess)
                return BadRequestResponse(getOwnProfileResult.Error ?? "Failed to get profile");

            // Step 3: Mapping hasil query ke response DTO API.
            var userProfile = getOwnProfileResult.Value!;
            var defaultAddressResponse = userProfile.DefaultAddress != null
                ? new AddressResponse(
                    userProfile.DefaultAddress.Id,
                    userProfile.DefaultAddress.Label,
                    userProfile.DefaultAddress.RecipientName,
                    userProfile.DefaultAddress.RecipientPhone,
                    userProfile.DefaultAddress.FullAddress,
                    userProfile.DefaultAddress.City,
                    userProfile.DefaultAddress.Province,
                    userProfile.DefaultAddress.PostalCode,
                    userProfile.DefaultAddress.IsDefault
                )
                : null;

            var userProfileResponse = new UserProfileResponse(
                userProfile.Id,
                userProfile.FullName,
                userProfile.Email,
                userProfile.Phone,
                userProfile.IsEmailVerified,
                userProfile.IsActive,
                userProfile.RoleName,
                userProfile.Avatar,
                userProfile.DateOfBirth,
                userProfile.Gender?.ToString(),
                userProfile.LastLoginAt,
                defaultAddressResponse,
                userProfile.MemberSince
            );

            // Step 4: Kembalikan response sukses.
            return SuccessResponse(userProfileResponse, "Success get profile");
        }

        /// <summary>
        /// Mengirim kode OTP verifikasi email ke email user yang sedang login.
        /// </summary>
        /// <remarks>
        /// Endpoint: POST api/users/me/email-verification/send
        ///
        /// Flow send verification code:
        /// 1. Endpoint memerlukan permission Users.UpdateOwn.
        /// 2. Controller memanggil handler untuk generate OTP verifikasi.
        /// 3. OTP disimpan dan dikirim ke email user.
        /// 4. Jika gagal, kembalikan HTTP 400.
        /// 5. Jika berhasil, kembalikan waktu kedaluwarsa OTP.
        /// </remarks>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse status pengiriman OTP verifikasi email.</returns>
        [HttpPost("me/email-verification/send")]
        [HasPermission(Permissions.Users.UpdateOwn)]
        [EnableRateLimiting("email-verify-send")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SendEmailVerificationCode(CancellationToken cancellationToken)
        {
            // Step 1: Bentuk command untuk kirim OTP verifikasi email.
            var sendEmailVerificationCommand = new SendEmailVerificationCommand();

            // Step 2: Eksekusi proses kirim OTP.
            var sendEmailVerificationResult = await _sendEmailVerificationHandler
                .Handle(sendEmailVerificationCommand, cancellationToken);

            // Step 3: Jika gagal, kirim response error.
            if (!sendEmailVerificationResult.IsSuccess)
                return BadRequestResponse(sendEmailVerificationResult.Error ?? "Failed to send verification code");

            // Step 4: Kembalikan response sukses + waktu kedaluwarsa.
            return SuccessResponse(
                new { expiresAt = sendEmailVerificationResult.Value },
                "Verification code sent successfully");
        }

        /// <summary>
        /// Memverifikasi email user login menggunakan kode OTP.
        /// </summary>
        /// <remarks>
        /// Endpoint: POST api/users/me/email-verification/verify
        ///
        /// Flow verify email:
        /// 1. Endpoint memerlukan permission Users.UpdateOwn.
        /// 2. Terima OTP dari request body.
        /// 3. Handler validasi OTP dan menandai email user sebagai verified.
        /// 4. Jika gagal, kembalikan HTTP 400.
        /// 5. Jika berhasil, kembalikan status verifikasi email.
        /// </remarks>
        /// <param name="verifyEmailRequest">Payload verifikasi email yang berisi OTP code.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse status verifikasi email.</returns>
        [HttpPost("me/email-verification/verify")]
        [HasPermission(Permissions.Users.UpdateOwn)]
        [EnableRateLimiting("email-verify-check")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> VerifyEmail(
            VerifyEmailRequest verifyEmailRequest,
            CancellationToken cancellationToken)
        {
            // Step 1: Mapping request ke command aplikasi.
            var verifyEmailCommand = new VerifyEmailCommand(verifyEmailRequest.Code);

            // Step 2: Eksekusi proses verifikasi OTP.
            var verifyEmailResult = await _verifyEmailHandler.Handle(verifyEmailCommand, cancellationToken);

            // Step 3: Jika gagal, kirim response error.
            if (!verifyEmailResult.IsSuccess)
                return BadRequestResponse(verifyEmailResult.Error ?? "Email verification failed");

            // Step 4: Kembalikan response sukses.
            return SuccessResponse(new { isEmailVerified = true }, "Email verified successfully");
        }

        /// <summary>
        /// Memperbarui profil pengguna yang sedang login.
        /// </summary>
        /// <remarks>
        /// Endpoint: PATCH api/users/me
        ///
        /// Flow update own profile:
        /// 1. Endpoint memerlukan permission Users.UpdateOwn.
        /// 2. Terima payload perubahan profil dari body request.
        /// 3. Bentuk UpdateOwnProfileCommand untuk Application Layer.
        /// 4. Handler memproses validasi dan pembaruan data profil.
        /// 5. Jika sukses, kembalikan userId yang diperbarui.
        /// </remarks>
        /// <param name="updateOwnProfileRequest">Payload perubahan data profil pengguna.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse status pembaruan profil.</returns>
        [HttpPatch("me")]
        [HasPermission(Permissions.Users.UpdateOwn)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [SwaggerRequestExample(typeof(UpdateOwnProfileRequest), typeof(UpdateOwnProfileRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(UpdateOwnProfileSuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(UpdateOwnProfileBadRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedExample))]
        public async Task<IActionResult> UpdateOwnProfile(
            UpdateOwnProfileRequest updateOwnProfileRequest,
            CancellationToken cancellationToken)
        {
            // Step 1: Mapping request ke command aplikasi.
            var updateOwnProfileCommand = new UpdateOwnProfileCommand(
                updateOwnProfileRequest.FullName,
                updateOwnProfileRequest.Email,
                updateOwnProfileRequest.Phone,
                updateOwnProfileRequest.Avatar,
                updateOwnProfileRequest.DateOfBirth,
                updateOwnProfileRequest.Gender
            );

            // Step 2: Eksekusi business logic update profil.
            var updateOwnProfileResult = await _updateOwnProfileHandler.Handle(updateOwnProfileCommand, cancellationToken);

            // Step 3: Jika gagal, kirim response error.
            if (!updateOwnProfileResult.IsSuccess)
                return BadRequestResponse(updateOwnProfileResult.Error ?? "Update profile failed");

            // Step 4: Jika sukses, kirim userId yang ter-update.
            return SuccessResponse(
                new { userId = updateOwnProfileResult.Value },
                "Profile updated successfully"
            );
        }

        /// <summary>
        /// Menghapus akun pengguna yang sedang login secara permanen.
        /// </summary>
        /// <remarks>
        /// Endpoint: DELETE api/users/me
        ///
        /// Flow delete own account:
        /// 1. Endpoint memerlukan permission Users.DeleteOwn.
        /// 2. Controller membuat DeleteOwnAccountCommand.
        /// 3. Handler mengeksekusi penghapusan akun milik user login.
        /// 4. Jika gagal, kembalikan HTTP 400.
        /// 5. Jika sukses, kembalikan userId akun yang dihapus.
        /// </remarks>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse status penghapusan akun.</returns>
        [HttpDelete("me")]
        [HasPermission(Permissions.Users.DeleteOwn)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(DeleteOwnAccountSuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(DeleteOwnAccountBadRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedExample))]
        public async Task<IActionResult> DeleteMyAccount(
            CancellationToken cancellationToken)
        {
            // Step 1: Bentuk command penghapusan akun.
            var deleteOwnAccountCommand = new DeleteOwnAccountCommand();

            // Step 2: Eksekusi proses delete akun.
            var deleteOwnAccountResult = await _deleteOwnAccountHandler.Handle(deleteOwnAccountCommand, cancellationToken);

            // Step 3: Jika gagal, kirim response error.
            if (!deleteOwnAccountResult.IsSuccess)
                return BadRequestResponse(deleteOwnAccountResult.Error ?? "Failed to delete account");

            // Step 4: Jika sukses, kirim userId akun yang dihapus.
            return SuccessResponse(
                new { userId = deleteOwnAccountResult.Value },
                "Account deleted permanently. You can register again with the same email.");
        }
    }
}

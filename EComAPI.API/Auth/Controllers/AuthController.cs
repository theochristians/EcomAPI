using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Swashbuckle.AspNetCore.Filters;
using EComAPI.API.Common;
using EComAPI.API.Auth.Dtos.Request;
using EComAPI.API.Auth.Dtos.Response;
using EComAPI.API.Auth.Swagger.Examples;
using EComAPI.API.Auth.Swagger.Examples.LoginExample.Request;
using EComAPI.API.Auth.Swagger.Examples.LoginExample.Response;
using EComAPI.API.Auth.Swagger.Examples.LogoutExample.Request;
using EComAPI.API.Auth.Swagger.Examples.LogoutExample.Response;
using EComAPI.API.Auth.Swagger.Examples.RefreshTokenExample.Request;
using EComAPI.API.Auth.Swagger.Examples.RefreshTokenExample.Response;
using EComAPI.API.Auth.Swagger.Examples.RegisterExample.Request;
using EComAPI.API.Auth.Swagger.Examples.RegisterExample.Response;
using EComAPI.Application.Auth.Commands.LoginUser;
using EComAPI.Application.Auth.Commands.LogoutUser;
using EComAPI.Application.Auth.Commands.ForgotPassword;
using EComAPI.Application.Auth.Commands.RefreshTokens;
using EComAPI.Application.Auth.Commands.RegisterUser;
using EComAPI.Application.Auth.Commands.ResetPassword;
using EComAPI.Application.Auth.Commands.SendEmailVerificationByEmail;
using EComAPI.Application.Auth.Commands.VerifyEmailByEmail;
using EComAPI.API.Common.Security;
using EComAPI.API.Common.Time;

namespace EComAPI.API.Auth.Controllers
{
    /// <summary>
    /// Endpoint autentikasi untuk API.
    /// Menyediakan proses registrasi, login, refresh token, dan logout.
    /// </summary>
    /// <remarks>
    /// Alur umum (high-level flow):
    /// 1. Controller menerima request HTTP dari client.
    /// 2. Data request dipetakan ke command aplikasi (Application Layer).
    /// 3. Handler mengeksekusi business logic autentikasi.
    /// 4. Hasil dikembalikan dalam format ApiResponse yang konsisten.
    /// </remarks>
    [ApiController]
    [Route("api/auth")]
    [Produces("application/json")]
    public class AuthController : BaseController
    {
        private readonly RegisterUserHandler _registerUserHandler;
        private readonly LoginUserHandler _loginUserHandler;
        private readonly RefreshTokenHandler _refreshTokenHandler;
        private readonly LogoutUserHandler _logoutUserHandler;
        private readonly SendEmailVerificationByEmailHandler _sendEmailVerificationByEmailHandler;
        private readonly VerifyEmailByEmailHandler _verifyEmailByEmailHandler;
        private readonly ForgotPasswordHandler _forgotPasswordHandler;
        private readonly ResetPasswordHandler _resetPasswordHandler;

        /// <summary>
        /// Inisialisasi AuthController dengan seluruh handler autentikasi yang dibutuhkan.
        /// </summary>
        /// <param name="registerUserHandler">Handler untuk proses registrasi akun baru.</param>
        /// <param name="loginUserHandler">Handler untuk proses login pengguna.</param>
        /// <param name="refreshTokenHandler">Handler untuk proses refresh access token.</param>
        /// <param name="logoutUserHandler">Handler untuk proses logout dan revokasi token.</param>
        /// <param name="sendEmailVerificationByEmailHandler">Handler kirim OTP verifikasi email tanpa login.</param>
        /// <param name="verifyEmailByEmailHandler">Handler verifikasi email dengan email + OTP tanpa login.</param>
        /// <param name="forgotPasswordHandler">Handler request OTP forgot password.</param>
        /// <param name="resetPasswordHandler">Handler reset password menggunakan OTP.</param>
        public AuthController(
            RegisterUserHandler registerUserHandler,
            LoginUserHandler loginUserHandler,
            RefreshTokenHandler refreshTokenHandler,
            LogoutUserHandler logoutUserHandler,
            SendEmailVerificationByEmailHandler sendEmailVerificationByEmailHandler,
            VerifyEmailByEmailHandler verifyEmailByEmailHandler,
            ForgotPasswordHandler forgotPasswordHandler,
            ResetPasswordHandler resetPasswordHandler)
        {
            _registerUserHandler = registerUserHandler;
            _loginUserHandler = loginUserHandler;
            _refreshTokenHandler = refreshTokenHandler;
            _logoutUserHandler = logoutUserHandler;
            _sendEmailVerificationByEmailHandler = sendEmailVerificationByEmailHandler;
            _verifyEmailByEmailHandler = verifyEmailByEmailHandler;
            _forgotPasswordHandler = forgotPasswordHandler;
            _resetPasswordHandler = resetPasswordHandler;
        }

        /// <summary>
        /// Mendaftarkan akun pengguna baru.
        /// </summary>
        /// <remarks>
        /// Endpoint: POST api/auth/register
        ///
        /// Flow register:
        /// 1. Ambil input nama lengkap, email, dan password dari request.
        /// 2. Bentuk RegisterUserCommand untuk dikirim ke Application Layer.
        /// 3. Handler melakukan validasi dan pembuatan user baru.
        /// 4. Jika gagal, kembalikan HTTP 400.
        /// 5. Jika berhasil, kembalikan data user yang baru terdaftar.
        ///
        /// Catatan:
        /// - Endpoint ini dapat diakses tanpa login (AllowAnonymous).
        /// - Respons sukses tidak mengembalikan password atau data sensitif.
        /// </remarks>
        /// <param name="registerRequest">Payload registrasi dari client.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse berisi hasil registrasi user.</returns>
        [HttpPost]
        [Route("register")]
        [AllowAnonymous]
        [EnableRateLimiting("auth-register")]
        [ProducesResponseType(typeof(ApiResponse<RegisterResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [SwaggerRequestExample(typeof(RegisterRequest), typeof(RegisterRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(RegisterSuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(RegisterBadRequestExample))]
        public async Task<IActionResult> Register(
            RegisterRequest registerRequest,
            CancellationToken cancellationToken)
        {
            // Step 1: Mapping request HTTP ke command aplikasi.
            var registerUserCommand = new RegisterUserCommand(
                registerRequest.FullName,
                registerRequest.Email,
                registerRequest.Password,
                registerRequest.Phone,
                registerRequest.DateOfBirth,
                registerRequest.Gender
            );

            // Step 2: Eksekusi business logic registrasi di handler.
            var registeredResult = await _registerUserHandler.Handle(
                registerUserCommand,
                cancellationToken);

            // Step 3: Jika gagal, kirim error response yang konsisten.
            if (!registeredResult.IsSuccess)
                return BadRequestResponse(registeredResult.Error ?? "User registration failed");

            var registeredUserDto = registeredResult.Value!;

            // Step 4: Jika berhasil, kirim data user baru sebagai response.
            return SuccessResponse(
                new RegisterResponse(
                    registeredUserDto.Id,
                    registeredUserDto.FullName,
                    registeredUserDto.Email,
                    registeredUserDto.IsEmailVerified
                ),
                "User registered successfully. Please verify email before login"
            );
        }

        /// <summary>
        /// Mengirim OTP verifikasi email berdasarkan email (tanpa login).
        /// </summary>
        /// <remarks>
        /// Endpoint: POST api/auth/email-verification/send
        /// </remarks>
        [HttpPost]
        [Route("email-verification/send")]
        [AllowAnonymous]
        [EnableRateLimiting("email-verify-send")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SendEmailVerificationByEmail(
            SendEmailVerificationByEmailRequest sendEmailVerificationByEmailRequest,
            CancellationToken cancellationToken)
        {
            var sendCommand = new SendEmailVerificationByEmailCommand(sendEmailVerificationByEmailRequest.Email);
            var sendResult = await _sendEmailVerificationByEmailHandler.Handle(sendCommand, cancellationToken);

            if (!sendResult.IsSuccess)
                return BadRequestResponse(sendResult.Error ?? "Failed to send verification code");

            return SuccessResponse(
                new { expiresAt = ApiTime.ToJakartaOffset(sendResult.Value) },
                "If the email is registered, verification code has been sent");
        }

        /// <summary>
        /// Verifikasi email menggunakan email + OTP (tanpa login).
        /// </summary>
        /// <remarks>
        /// Endpoint: POST api/auth/email-verification/verify
        /// </remarks>
        [HttpPost]
        [Route("email-verification/verify")]
        [AllowAnonymous]
        [EnableRateLimiting("email-verify-check")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerifyEmailByEmail(
            VerifyEmailByEmailRequest verifyEmailByEmailRequest,
            CancellationToken cancellationToken)
        {
            var verifyCommand = new VerifyEmailByEmailCommand(
                verifyEmailByEmailRequest.Email,
                verifyEmailByEmailRequest.Code);

            var verifyResult = await _verifyEmailByEmailHandler.Handle(verifyCommand, cancellationToken);
            if (!verifyResult.IsSuccess)
                return BadRequestResponse(verifyResult.Error ?? "Email verification failed");

            return SuccessResponse(new { isEmailVerified = true }, "Email verified successfully");
        }

        /// <summary>
        /// Mengautentikasi pengguna dan mengembalikan access token + refresh token.
        /// </summary>
        /// <remarks>
        /// Endpoint: POST api/auth/login
        ///
        /// Flow login:
        /// 1. Terima email, password, dan informasi device dari request.
        /// 2. Ambil metadata request (IP Address dan User-Agent).
        /// 3. Bentuk LoginUserCommand lalu kirim ke handler.
        /// 4. Jika kredensial invalid, kembalikan HTTP 400.
        /// 5. Jika valid, kembalikan access token dan refresh token.
        ///
        /// Catatan keamanan:
        /// - Access Token (JWT) kedaluwarsa sesuai konfigurasi JwtSettings:ExpiryMinutes.
        /// - Refresh Token berumur lebih panjang (7 hari).
        /// - Simpan refresh token secara aman di sisi client.
        /// </remarks>
        /// <param name="loginRequest">Payload login dari client.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse berisi token autentikasi.</returns>
        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        [EnableRateLimiting("auth-login")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [SwaggerRequestExample(typeof(LoginRequest), typeof(LoginRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(LoginSuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(LoginBadRequestExample))]
        public async Task<IActionResult> Login(
            LoginRequest loginRequest,
            CancellationToken cancellationToken)
        {
            // Step 1: Ambil metadata request untuk kebutuhan audit/tracking device.
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = Request.Headers["User-Agent"].ToString();

            // Step 2: Mapping request ke command aplikasi.
            var loginUserCommand = new LoginUserCommand(
                loginRequest.Email,
                loginRequest.Password,
                ipAddress,
                userAgent,
                loginRequest.DeviceName
            );

            // Step 3: Eksekusi proses autentikasi.
            var loginResult = await _loginUserHandler.Handle(
                loginUserCommand,
                cancellationToken);

            // Step 4: Jika gagal, kirim response error.
            if (!loginResult.IsSuccess)
                return BadRequestResponse(loginResult.Error ?? "Invalid credentials");

            var loginDto = loginResult.Value!;

            // Step 5: Jika berhasil, kirim access token dan refresh token.
            return SuccessResponse(
                new LoginResponse(
                    loginDto.AccessToken,
                    loginDto.RefreshToken,
                    loginDto.AccessTokenExpiresAt,
                    loginDto.RefreshTokenExpiresAt
                ),
                "Login successful"
            );
        }

        /// <summary>
        /// Meminta OTP reset password berdasarkan email.
        /// </summary>
        /// <remarks>
        /// Endpoint: POST api/auth/forgot-password
        /// </remarks>
        [HttpPost]
        [Route("forgot-password")]
        [AllowAnonymous]
        [EnableRateLimiting("email-verify-send")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ForgotPassword(
            ForgotPasswordRequest forgotPasswordRequest,
            CancellationToken cancellationToken)
        {
            var forgotCommand = new ForgotPasswordCommand(forgotPasswordRequest.Email);
            var forgotResult = await _forgotPasswordHandler.Handle(forgotCommand, cancellationToken);

            if (!forgotResult.IsSuccess)
                return BadRequestResponse(forgotResult.Error ?? "Failed to process forgot password");

            return SuccessResponse(
                new { expiresAt = ApiTime.ToJakartaOffset(forgotResult.Value) },
                "If the email is registered, reset code has been sent");
        }

        /// <summary>
        /// Reset password menggunakan email + OTP + password baru.
        /// </summary>
        /// <remarks>
        /// Endpoint: POST api/auth/reset-password
        /// </remarks>
        [HttpPost]
        [Route("reset-password")]
        [AllowAnonymous]
        [EnableRateLimiting("email-verify-check")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword(
            ResetPasswordRequest resetPasswordRequest,
            CancellationToken cancellationToken)
        {
            var resetCommand = new ResetPasswordCommand(
                resetPasswordRequest.Email,
                resetPasswordRequest.Code,
                resetPasswordRequest.NewPassword);

            var resetResult = await _resetPasswordHandler.Handle(resetCommand, cancellationToken);
            if (!resetResult.IsSuccess)
                return BadRequestResponse(resetResult.Error ?? "Password reset failed");

            return SuccessResponse(new { isPasswordReset = true }, "Password reset successfully");
        }

        /// <summary>
        /// Memperbarui access token menggunakan refresh token.
        /// </summary>
        /// <remarks>
        /// Endpoint: POST api/auth/refresh-token
        ///
        /// Flow refresh token:
        /// 1. Terima refresh token dari body request.
        /// 2. Ambil access token saat ini dari Authorization header (jika ada).
        /// 3. Ambil metadata request (IP, User-Agent, nama device).
        /// 4. Handler memvalidasi token dan melakukan token rotation.
        /// 5. Jika sukses, kembalikan pasangan access token + refresh token baru.
        ///
        /// Catatan:
        /// - Endpoint ini dipakai saat access token kedaluwarsa.
        /// - Refresh token lama akan dinonaktifkan setelah rotasi berhasil.
        /// </remarks>
        /// <param name="refreshTokenRequest">Payload yang berisi refresh token.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse berisi token baru setelah rotasi.</returns>
        [HttpPost]
        [Route("refresh-token")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [SwaggerRequestExample(typeof(RefreshTokenRequest), typeof(RefreshTokenRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(RefreshTokenSuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(RefreshTokenBadRequestExample))]
        public async Task<IActionResult> RefreshToken(
            RefreshTokenRequest refreshTokenRequest,
            CancellationToken cancellationToken)
        {
            // Step 1: Ambil bearer token dari header Authorization.
            var authHeader = Request.Headers["Authorization"].ToString();
            var currentAccessToken = AuthorizationHeaderHelper.ExtractBearerToken(authHeader);

            // Step 2: Mapping data request + metadata menjadi command.
            var refreshTokenCommand = new RefreshTokenCommand(
                refreshTokenRequest.RefreshToken,
                currentAccessToken,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                Request.Headers["User-Agent"].ToString(),
                Request.Headers["X-Device-Name"].ToString()
            );

            // Step 3: Eksekusi proses refresh token di handler.
            var refreshTokenResult = await _refreshTokenHandler.Handle(refreshTokenCommand, cancellationToken);

            // Step 4: Jika gagal, kirim response error.
            if (!refreshTokenResult.IsSuccess)
                return BadRequestResponse(refreshTokenResult.Error ?? "Refresh token failed");

            var loginDto = refreshTokenResult.Value!;

            // Step 5: Jika berhasil, kirim token baru hasil rotasi.
            return SuccessResponse(
                new LoginResponse(
                    loginDto.AccessToken,
                    loginDto.RefreshToken,
                    loginDto.AccessTokenExpiresAt,
                    loginDto.RefreshTokenExpiresAt
                ),
                "Token refreshed successfully"
            );
        }


        /// <summary>
        /// Logout pengguna (revokasi token sesi aktif).
        /// </summary>
        /// <remarks>
        /// Endpoint: POST api/auth/logout
        ///
        /// Flow logout:
        /// 1. Endpoint membutuhkan user terautentikasi (Authorize).
        /// 2. Ambil access token dari Authorization header.
        /// 3. Terima refresh token dari request body (opsional tergantung implementasi).
        /// 4. Handler melakukan revokasi refresh token dan invalidasi sesi.
        /// 5. Jika sukses, token tidak bisa dipakai lagi untuk akses API.
        /// </remarks>
        /// <param name="logoutRequest">Payload logout yang memuat refresh token.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse status logout.</returns>
        [HttpPost]
        [Route("logout")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [SwaggerRequestExample(typeof(LogoutRequest), typeof(LogoutRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(LogoutSuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(LogoutBadRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedExample))]
        public async Task<IActionResult> Logout(
            LogoutRequest logoutRequest,
            CancellationToken cancellationToken)
        {
            // Step 1: Ambil access token dari Authorization header.
            var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
            var accessToken = AuthorizationHeaderHelper.ExtractBearerToken(authHeader);

            // Step 2: Mapping request ke command aplikasi.
            var logoutUserCommand = new LogoutUserCommand(
                logoutRequest.RefreshToken,
                accessToken
            );

            // Step 3: Eksekusi proses logout/revokasi token.
            var logoutUserResult = await _logoutUserHandler.Handle(logoutUserCommand, cancellationToken);

            // Step 4: Jika gagal, kirim response error.
            if (!logoutUserResult.IsSuccess)
                return BadRequestResponse(logoutUserResult.Error ?? "Logout failed");

            // Step 5: Jika sukses, kirim status logout berhasil.
            return SuccessResponse(
                new { message = "Logged out successfully" },
                "Logout successful"
            );
        }
    }
}



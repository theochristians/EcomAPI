using EComAPI.API.Auth.Dtos.Response;
using EComAPI.API.Auth.DTOs.Requests;
using EComAPI.API.Auth.DTOs.Responses;
using EComAPI.API.Auth.Swagger.Examples;
using EComAPI.API.Auth.Swagger.Examples.LoginExample.Request;
using EComAPI.API.Auth.Swagger.Examples.LoginExample.Response;
using EComAPI.API.Auth.Swagger.Examples.MeExample.Response;
using EComAPI.API.Auth.Swagger.Examples.RegisterExample.Request;
using EComAPI.API.Auth.Swagger.Examples.RegisterExample.Response;
using EComAPI.API.Common;
using EComAPI.API.Common.Extensions;
using EComAPI.Application.Auth.Commands.LoginUser;
using EComAPI.Application.Auth.Commands.RegisterUser;
using EComAPI.Application.Auth.Queries.GetCurrentUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Auth.Controllers

{
    /// <summary>
    /// Authentication endpoints
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    [Produces("application/json")]
    public class AuthController : BaseController
    {
        private readonly RegisterUserHandler _registerUserHandler;
        private readonly LoginUserHandler _loginUserHandler;
        private readonly GetCurrentUserHandler _getCurrentUserHandler;

        public AuthController(
            RegisterUserHandler registerUserHandler,
            LoginUserHandler loginUserHandler,
            GetCurrentUserHandler getCurrentUserHandler)
        {
            _registerUserHandler = registerUserHandler;
            _loginUserHandler = loginUserHandler;
            _getCurrentUserHandler = getCurrentUserHandler;
        }

        /// <summary>
        /// Register new user
        /// </summary>
        /// <remarks>
        ///     POST api/auth/register
        /// </remarks>
        /// <response code="200">User registered successfully</response>
        /// <response code="400">Email already registered</response>
        /// <response code="401">Unauthorized</response>
        [HttpPost]
        [Route("register")]
        [AllowAnonymous]
        // Response
        [ProducesResponseType(typeof(ApiResponse<RegisterResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        // SwaggerExample
        [SwaggerRequestExample(typeof(RegisterRequest), typeof(RegisterRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(RegisterSuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(RegisterBadRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedExample))]
        public async Task<IActionResult> Register(RegisterRequest registerRequest)
        {
            var registerUserCommand = new RegisterUserCommand(
                registerRequest.FullName,
                registerRequest.Email,
                registerRequest.Password
            );

            var registedResult = await _registerUserHandler.Handle(registerUserCommand);

            if (!registedResult.IsSuccess)
                return BadRequestResponse("User registered failed");

            var registeredUserDto = registedResult.Value!;

            return SuccesResponse(
                new RegisterResponse(
                    registeredUserDto.Id,
                    registeredUserDto.FullName,
                    registeredUserDto.Email,
                    registeredUserDto.IsEmailVerified
                ),
                "User registered successfully"
            );
        }

        /// <summary>
        /// Login user
        /// </summary>
        /// <remarks>
        ///     POST api/auth/login
        /// </remarks>
        /// <response code="200">Login success</response>
        /// <response code="400">Invalid credentials</response>
        /// <response code="401">Unauthorized</response>
        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        // Response
        [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        // SwaggerExample
        [SwaggerRequestExample(typeof(LoginRequest), typeof(LoginRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(LoginSuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(LoginBadRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedExample))]
        public async Task<IActionResult> Login(LoginRequest loginRequest)
        {
            var loginUserCommand = new LoginUserCommand(
                loginRequest.Email,
                loginRequest.Password
            );

            var loginResult = await _loginUserHandler.Handle(loginUserCommand);

            if (!loginResult.IsSuccess)
                return UnauthorizedResponse("Login failed");

            return SuccesResponse(
                new AuthResponse(loginResult.Value!),
                "Login successful"
            );
        }
        /// <summary>
        /// Login user
        /// </summary>
        /// <remarks>
        ///     POST api/auth/login
        /// </remarks>
        /// <response code="200">Login success</response>
        /// <response code="400">Invalid credentials</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [Route("me")]
        [Authorize]
        // Response
        [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        // SwaggerExample
        [SwaggerRequestExample(typeof(LoginRequest), typeof(LoginRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(MeSuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedExample))]
        public async Task<IActionResult> Me()
        {
            var userId = User.GetUserId();
            var userResult = await _getCurrentUserHandler.Handle(new(userId));

            if (!userResult.IsSuccess)
                return BadRequestResponse("Failed get my profile");

            return SuccesResponse(
                new UserProfileResponse(
                    userResult.Value!.Id,
                    userResult.Value.FullName,
                    userResult.Value.Email,
                    userResult.Value.IsEmailVerified
                ),
                "Current my profile"
            );

        }

    }

}
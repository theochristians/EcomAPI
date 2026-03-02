namespace EComAPI.Application.Auth.Commands.LoginUser
{
    public record LoginUserCommand(
        string Email,
        string Password,
        string? IpAddress = null,
        string? UserAgent = null,
        string? DeviceName = null
    );
}
namespace EComAPI.Application.Auth.Commands.RefreshTokens
{
    public record RefreshTokenCommand(
        string RefreshToken,
        string? CurrentAccessToken = null,
        string? IpAddress = null,
        string? UserAgent = null,
        string? DeviceName = null
    );
}

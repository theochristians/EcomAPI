namespace EComAPI.Application.Auth.DTOs
{
    public record LoginUserDto(
        string AccessToken,
        string RefreshToken,
        DateTime AccessTokenExpiresAt,
        DateTime RefreshTokenExpiresAt
    );
}

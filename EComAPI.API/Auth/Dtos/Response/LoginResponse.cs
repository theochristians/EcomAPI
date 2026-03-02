namespace EComAPI.API.Auth.Dtos.Response
{
    public record LoginResponse(
        string AccessToken,
        string RefreshToken,
        DateTime AccessTokenExpiresAt,
        DateTime RefreshTokenExpiresAt
    );
}
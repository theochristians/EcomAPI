namespace EComAPI.API.Auth.Dtos.Request
{
    public record LogoutRequest(string? RefreshToken = null);
}

namespace EComAPI.API.Auth.Dtos.Request
{
    public record LoginRequest(
        string Email,
        string Password,
        string? DeviceName = null
    );
}
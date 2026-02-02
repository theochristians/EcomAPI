namespace EComAPI.API.Auth.DTOs.Requests
{
    public record LoginRequest(
        string Email,
        string Password
    );
}
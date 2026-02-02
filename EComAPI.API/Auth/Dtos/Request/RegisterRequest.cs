namespace EComAPI.API.Auth.DTOs.Requests
{
    public record RegisterRequest(
        string FullName,
        string Email,
        string Password
    );
}
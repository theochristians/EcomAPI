namespace EComAPI.API.Auth.Dtos.Request
{
    public record RegisterRequest(
        string FullName,
        string Email,
        string Password
    );
}
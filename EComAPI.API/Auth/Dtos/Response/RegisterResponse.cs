namespace EComAPI.API.Auth.DTOs.Responses
{
    public record RegisterResponse(
        Guid UserId,
        string FullName,
        string Email,
        bool IsEmailVerified
    );
}
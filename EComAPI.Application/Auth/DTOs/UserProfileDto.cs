namespace EComAPI.Application.Auth.DTOs
{
    public record UserProfileDto(
        Guid Id,
        string FullName,
        string Email,
        bool IsEmailVerified
    );
}
namespace EComAPI.Application.Auth.DTOs
{
    public record RegisteredUserDto(
        Guid Id,
        string FullName,
        string Email,
        bool IsEmailVerified
    );
}
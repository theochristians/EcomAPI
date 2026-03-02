using EComAPI.Domain.Auth.Enums;

namespace EComAPI.Application.Auth.DTOs
{
    public record UserProfileDto(
        Guid Id,
        string FullName,
        string Email,
        string? Phone,
        bool IsEmailVerified,
        bool IsActive,
        string RoleName,
        string? Avatar,
        DateTime? DateOfBirth,
        Gender? Gender,
        DateTime? LastLoginAt,
        AddressDto? DefaultAddress,
        DateTime MemberSince
    );
}

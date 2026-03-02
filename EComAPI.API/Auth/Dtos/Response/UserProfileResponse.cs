using EComAPI.Application.Auth.DTOs;

namespace EComAPI.API.Auth.Dtos.Response
{
    public record UserProfileResponse(
        Guid Id,
        string FullName,
        string Email,
        string? Phone,
        bool IsEmailVerified,
        bool IsActive,
        string RoleName,
        string? Avatar,
        DateTime? DateOfBirth,
        string? Gender,  
        DateTime? LastLoginAt,
        AddressResponse? DefaultAddress,
        DateTime MemberSince
    );
}

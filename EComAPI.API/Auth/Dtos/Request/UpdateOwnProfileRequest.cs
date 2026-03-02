using EComAPI.Domain.Auth.Enums;

namespace EComAPI.API.Auth.Dtos.Request
{
    public record UpdateOwnProfileRequest(
        string? FullName,
        string? Email,
        string? Phone,
        string? Avatar,         
        DateTime? DateOfBirth,   
        Gender? Gender           
    );
}
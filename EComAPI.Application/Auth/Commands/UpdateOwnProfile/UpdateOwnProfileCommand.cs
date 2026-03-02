using EComAPI.Domain.Auth.Enums;

namespace EComAPI.Application.Auth.Commands.UpdateOwnProfile
{
    public record UpdateOwnProfileCommand(
        string? FullName,
        string? Email,
        string? Phone,
        string? Avatar,         
        DateTime? DateOfBirth,   
        Gender? Gender           
    );
}
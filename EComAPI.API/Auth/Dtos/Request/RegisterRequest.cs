using EComAPI.Domain.Auth.Enums;

namespace EComAPI.API.Auth.Dtos.Request
{
    public record RegisterRequest(
        string FullName,
        string Email,
        string Password,
        string Phone,
        DateTime DateOfBirth,
        Gender Gender
    );
}
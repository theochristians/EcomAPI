using EComAPI.Domain.Auth.Enums;

namespace EComAPI.Application.Auth.Commands.RegisterUser
{
    public record RegisterUserCommand(
        string FullName,
        string Email,
        string Password,
        string Phone,
        DateTime DateOfBirth,
        Gender Gender
    );
}
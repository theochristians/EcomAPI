namespace EComAPI.Application.Auth.Commands.RegisterUser
{
    public record RegisterUserCommand(
        string FullName,
        string Email,
        string Password
    );
}
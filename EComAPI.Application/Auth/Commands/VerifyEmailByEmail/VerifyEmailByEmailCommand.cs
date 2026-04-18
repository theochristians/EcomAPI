namespace EComAPI.Application.Auth.Commands.VerifyEmailByEmail
{
    public record VerifyEmailByEmailCommand(string Email, string Code);
}

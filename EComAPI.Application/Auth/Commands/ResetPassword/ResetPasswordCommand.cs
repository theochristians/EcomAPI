namespace EComAPI.Application.Auth.Commands.ResetPassword
{
    public record ResetPasswordCommand(
        string Email,
        string Code,
        string NewPassword);
}

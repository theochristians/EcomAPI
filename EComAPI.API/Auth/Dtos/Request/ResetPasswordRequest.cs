namespace EComAPI.API.Auth.Dtos.Request
{
    public record ResetPasswordRequest(
        string Email,
        string Code,
        string NewPassword);
}

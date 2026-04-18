namespace EComAPI.API.Auth.Dtos.Request
{
    public record VerifyEmailByEmailRequest(
        string Email,
        string Code);
}

namespace EComAPI.API.Auth.Dtos.Response
{
    public record RegisterResponse(
        Guid UserId,
        string FullName,
        string Email,
        bool IsEmailVerified
    );
}
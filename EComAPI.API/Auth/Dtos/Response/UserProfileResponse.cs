namespace EComAPI.API.Auth.Dtos.Response
{
    public record UserProfileResponse(
        Guid Id,
        string FullName,
        string Email,
        bool IsEmailVerified
    );
}

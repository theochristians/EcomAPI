namespace EComAPI.Application.Auth.Commands.LogoutUser
{
    public record LogoutUserCommand(
        string? RefreshToken = null,
        string? AccessToken = null
    );
}

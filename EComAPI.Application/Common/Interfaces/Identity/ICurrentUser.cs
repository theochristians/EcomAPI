namespace EComAPI.Application.Common.Interfaces.Identity
{
    public interface ICurrentUser
    {
        Guid UserId { get; }
        string? Email { get; }
        string? FullName { get; }
        bool IsAuthenticated { get; }
        bool HasPermission(string permission);
    }
}

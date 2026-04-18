using EComAPI.Domain.Auth.Entities;

namespace EComAPI.Application.Auth.Interfaces
{
    public interface IJwtTokenGenerator
    {
        // SECURITY SERVICE
        // Bukan repository. Tugasnya generate token dari user.
        Task<string> GenerateTokenAsync(User user);
    }
}

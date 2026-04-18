namespace EComAPI.Application.Auth.Interfaces
{
    public interface IPasswordHasher
    {
        // SECURITY SERVICE
        // Bukan repository. Tugasnya hash dan verify password.
        string Hash(string password);
        bool Verify(string password, string hash);
    }
}

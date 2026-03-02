namespace EComAPI.Application.Auth.Interfaces
{
    public interface IEmailSender
    {
        Task SendEmailVerificationCodeAsync(
            string toEmail,
            string fullName,
            string code,
            DateTime expiresAt,
            CancellationToken cancellationToken = default);
    }
}

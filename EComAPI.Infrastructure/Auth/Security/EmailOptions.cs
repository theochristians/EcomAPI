namespace EComAPI.Infrastructure.Auth.Security
{
    public sealed class EmailOptions
    {
        public const string SectionName = "Email";

        public bool Enabled { get; set; }
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public string SmtpUsername { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = "EComAPI";
        public bool EnableSsl { get; set; } = true;
        public string BrandName { get; set; } = "Thrifties";
        public string BrandDomain { get; set; } = "thrifties.app";
        public string LogoUrl { get; set; } = string.Empty;
    }
}

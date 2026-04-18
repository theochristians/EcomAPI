using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Text;
using EComAPI.Application.Auth.Interfaces;
using Microsoft.Extensions.Logging;

namespace EComAPI.Infrastructure.Auth.Security
{
    public class BrevoSmtpEmailSender : IEmailSender
    {
        private readonly EmailOptions _emailOptions;
        private readonly ILogger<BrevoSmtpEmailSender> _logger;

        public BrevoSmtpEmailSender(
            EmailOptions emailOptions,
            ILogger<BrevoSmtpEmailSender> logger)
        {
            _emailOptions = emailOptions;
            _logger = logger;
        }

        public async Task SendEmailVerificationCodeAsync(
            string toEmail,
            string fullName,
            string code,
            DateTime expiresAt,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var subject = $"Kode Verifikasi Email {_emailOptions.BrandName}";
            var htmlBody = BuildOtpEmailHtml(
                fullName,
                code,
                expiresAt,
                OtpEmailType.EmailVerification);

            await SendEmailAsync(toEmail, subject, htmlBody, cancellationToken);

            _logger.LogInformation(
                "Email verification code sent to {Email}. Expires at {ExpiresAtUtc:u}",
                toEmail,
                expiresAt);
        }

        public async Task SendPasswordResetCodeAsync(
            string toEmail,
            string fullName,
            string code,
            DateTime expiresAt,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var subject = $"Kode Reset Password {_emailOptions.BrandName}";
            var htmlBody = BuildOtpEmailHtml(
                fullName,
                code,
                expiresAt,
                OtpEmailType.PasswordReset);

            await SendEmailAsync(toEmail, subject, htmlBody, cancellationToken);

            _logger.LogInformation(
                "Password reset code sent to {Email}. Expires at {ExpiresAtUtc:u}",
                toEmail,
                expiresAt);
        }

        private async Task SendEmailAsync(
            string toEmail,
            string subject,
            string htmlBody,
            CancellationToken cancellationToken)
        {
            using var message = new MailMessage
            {
                From = new MailAddress(_emailOptions.FromEmail, _emailOptions.FromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8
            };

            message.To.Add(toEmail);

            using var smtpClient = new SmtpClient(_emailOptions.SmtpHost, _emailOptions.SmtpPort)
            {
                DeliveryMethod = SmtpDeliveryMethod.Network,
                EnableSsl = _emailOptions.EnableSsl,
                Credentials = new NetworkCredential(
                    _emailOptions.SmtpUsername,
                    _emailOptions.SmtpPassword)
            };

            await smtpClient.SendMailAsync(message, cancellationToken);
        }

        private string BuildOtpEmailHtml(
            string fullName,
            string code,
            DateTime expiresAt,
            OtpEmailType emailType)
        {
            var safeBrandName = WebUtility.HtmlEncode(_emailOptions.BrandName);
            var safeBrandDomain = WebUtility.HtmlEncode(_emailOptions.BrandDomain);
            var safeFullName = WebUtility.HtmlEncode(
                string.IsNullOrWhiteSpace(fullName) ? "Pengguna" : fullName.Trim());
            var safeCode = WebUtility.HtmlEncode(code?.Trim() ?? string.Empty);
            var safeExpiresAt = WebUtility.HtmlEncode(FormatJakartaTimestamp(expiresAt));
            var logoMarkup = BuildLogoMarkup(safeBrandName);

            var content = GetContentByType(emailType);
            var currentYear = JakartaTime.Now.Year.ToString(CultureInfo.InvariantCulture);

            const string template = """
<!DOCTYPE html>
<html lang="id">
<head>
  <meta charset="UTF-8"/>
  <meta name="viewport" content="width=device-width,initial-scale=1"/>
  <title>__BRAND_NAME__ - OTP</title>
  <style>
    * { box-sizing: border-box; margin: 0; padding: 0; }
    body {
      background: #ECEEE9;
      font-family: Helvetica, Arial, sans-serif;
      padding: 40px 16px;
      -webkit-font-smoothing: antialiased;
    }
    .wrap { max-width: 580px; margin: 0 auto; }
    .section-label {
      text-align: center;
      margin-bottom: 14px;
      color: #6B7D6C;
      font-size: 11px;
      font-weight: 700;
      letter-spacing: 1.5px;
      text-transform: uppercase;
    }
    .card {
      border-radius: 20px;
      overflow: hidden;
      box-shadow: 0 8px 40px rgba(0,0,0,.13);
    }
    .header {
      background: linear-gradient(160deg, #1A3320 0%, #243D2A 55%, #1C3222 100%);
      padding: 44px 40px 38px;
      text-align: center;
      position: relative;
      overflow: hidden;
    }
    .header::before {
      content: '';
      position: absolute;
      top: -80px; left: 50%;
      transform: translateX(-50%);
      width: 360px; height: 360px;
      background: radial-gradient(circle, rgba(78,160,100,.16) 0%, transparent 68%);
      pointer-events: none;
    }
    .header::after {
      content: '';
      position: absolute;
      bottom: 0; left: 0; right: 0;
      height: 1px;
      background: linear-gradient(90deg, transparent, rgba(78,160,100,.35), transparent);
    }
    .logo-wrap {
      position: relative;
      display: inline-flex;
      align-items: center;
      justify-content: center;
      margin-bottom: 20px;
    }
    .logo-glow {
      position: absolute;
      inset: -10px;
      border-radius: 26px;
      background: rgba(78,160,100,.15);
      filter: blur(8px);
    }
    .logo-box {
      position: relative;
      width: 64px; height: 64px;
      background: #122418;
      border-radius: 18px;
      border: 1.5px solid rgba(78,160,100,.35);
      display: flex;
      align-items: center;
      justify-content: center;
    }
    .logo-box svg {
      width: 34px; height: 34px;
    }
    .logo-img {
      position: relative;
      width: 64px;
      height: 64px;
      border-radius: 18px;
      border: 1.5px solid rgba(78,160,100,.35);
      background: #122418;
      object-fit: contain;
      padding: 8px;
      display: block;
    }
    .header h1 {
      color: #FFFFFF;
      font-size: 24px;
      font-weight: 800;
      letter-spacing: -0.5px;
      margin-bottom: 8px;
    }
    .domain-pill {
      display: inline-block;
      color: #6DBF7E;
      font-size: 12px;
      font-weight: 500;
      letter-spacing: 0.5px;
      background: rgba(78,160,100,.12);
      padding: 4px 14px;
      border-radius: 100px;
      border: 1px solid rgba(78,160,100,.28);
    }
    .body { background: #fff; padding: 34px 40px 38px; }
    .badge {
      display: inline-flex;
      align-items: center;
      gap: 7px;
      border-radius: 100px;
      padding: 6px 14px;
      margin-bottom: 24px;
      font-size: 11px;
      font-weight: 700;
      letter-spacing: 0.8px;
      text-transform: uppercase;
    }
    .badge.green { background: #EAF4EC; color: #2E7D42; }
    .badge.amber { background: #FEF3C7; color: #B45309; }
    .greeting { color: #111B13; font-size: 18px; font-weight: 700; margin-bottom: 6px; }
    .intro { color: #5C6B5E; font-size: 14px; line-height: 1.75; margin-bottom: 28px; }
    .otp-box {
      border-radius: 14px;
      padding: 26px 16px;
      text-align: center;
      margin-bottom: 20px;
    }
    .otp-box.green { background: #F4FAF5; border: 1.5px solid #3E7D4E; }
    .otp-box.amber { background: #FFFBF0; border: 1.5px solid #B45309; }
    .otp-label {
      font-size: 10px;
      font-weight: 700;
      letter-spacing: 2px;
      text-transform: uppercase;
      margin-bottom: 10px;
    }
    .otp-code {
      font-size: 46px;
      font-weight: 900;
      letter-spacing: 14px;
      line-height: 1;
    }
    .otp-box.green .otp-label,
    .otp-box.green .otp-code { color: #3E7D4E; }
    .otp-box.amber .otp-label,
    .otp-box.amber .otp-code { color: #B45309; }
    .expiry {
      padding: 13px 16px;
      border-radius: 0 8px 8px 0;
      margin-bottom: 28px;
      font-size: 13px;
      line-height: 1.6;
      color: #3E5041;
    }
    .expiry.green { background: #F4FAF5; border-left: 3px solid #3E7D4E; }
    .expiry.amber { background: #FFFBF0; border-left: 3px solid #B45309; }
    .divider { border-top: 1px solid #EAEDE9; margin-bottom: 20px; }
    .warning { color: #8A9B8C; font-size: 12px; line-height: 1.75; }
    .footer {
      background: #F5F7F4;
      padding: 22px 40px;
      text-align: center;
      border-top: 1px solid #E4E9E3;
    }
    .footer p { color: #8A9B8C; font-size: 12px; margin-bottom: 4px; }
    .footer small { color: #AABDAB; font-size: 11px; }
  </style>
</head>
<body>
  <div class="wrap">
    <div class="section-label">&#10022; __SECTION_LABEL__</div>
    <div class="card">
      <div class="header">
        __LOGO_MARKUP__
        <h1>__BRAND_NAME__</h1>
        <span class="domain-pill">__BRAND_DOMAIN__</span>
      </div>

      <div class="body">
        <div class="badge __BADGE_CLASS__">
          __BADGE_ICON__
          __SECTION_LABEL__
        </div>
        <p class="greeting">Halo, __FULL_NAME__!</p>
        <p class="intro">__INTRO__</p>

        <div class="otp-box __BADGE_CLASS__">
          <p class="otp-label">Kode OTP Kamu</p>
          <p class="otp-code">__OTP_CODE__</p>
        </div>

        <div class="expiry __BADGE_CLASS__">
          Berlaku hingga <strong>__EXPIRES_AT__</strong>
        </div>

        <div class="divider"></div>
        <p class="warning">__WARNING__</p>
      </div>

      <div class="footer">
        <p>&copy; __YEAR__ __BRAND_NAME__ - __BRAND_DOMAIN__</p>
        <small>Email ini dikirim otomatis, mohon tidak membalas.</small>
      </div>
    </div>
  </div>
</body>
</html>
""";

            return template
                .Replace("__SECTION_LABEL__", content.SectionLabel, StringComparison.Ordinal)
                .Replace("__BADGE_CLASS__", content.BadgeClass, StringComparison.Ordinal)
                .Replace("__BADGE_ICON__", content.BadgeIconSvg, StringComparison.Ordinal)
                .Replace("__INTRO__", content.IntroText, StringComparison.Ordinal)
                .Replace("__WARNING__", content.WarningText, StringComparison.Ordinal)
                .Replace("__OTP_CODE__", safeCode, StringComparison.Ordinal)
                .Replace("__EXPIRES_AT__", safeExpiresAt, StringComparison.Ordinal)
                .Replace("__FULL_NAME__", safeFullName, StringComparison.Ordinal)
                .Replace("__BRAND_NAME__", safeBrandName, StringComparison.Ordinal)
                .Replace("__BRAND_DOMAIN__", safeBrandDomain, StringComparison.Ordinal)
                .Replace("__LOGO_MARKUP__", logoMarkup, StringComparison.Ordinal)
                .Replace("__YEAR__", currentYear, StringComparison.Ordinal);
        }

        private string BuildLogoMarkup(string safeBrandName)
        {
            if (Uri.TryCreate(_emailOptions.LogoUrl, UriKind.Absolute, out var logoUri)
                && (logoUri.Scheme == Uri.UriSchemeHttps || logoUri.Scheme == Uri.UriSchemeHttp))
            {
                var safeLogoUrl = WebUtility.HtmlEncode(logoUri.ToString());

                return $$"""
<div class="logo-wrap">
  <div class="logo-glow"></div>
  <img class="logo-img" src="{{safeLogoUrl}}" alt="{{safeBrandName}} logo" />
</div>
""";
            }

            return """
<div class="logo-wrap">
  <div class="logo-glow"></div>
  <div class="logo-box">
    <svg viewBox="0 0 34 34" fill="none" xmlns="http://www.w3.org/2000/svg">
      <path d="M12 13V10.5a5 5 0 0 1 10 0V13" stroke="#4EA064" stroke-width="2" stroke-linecap="round" fill="none"/>
      <rect x="6" y="13" width="22" height="17" rx="3" stroke="#4EA064" stroke-width="2" fill="none"/>
      <path d="M17 19.5 a3 3 0 1 1 -2.6 1.5" stroke="#4EA064" stroke-width="1.6" stroke-linecap="round" fill="none"/>
      <polyline points="14.4,21 14.4,23 12.4,23" stroke="#4EA064" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round" fill="none"/>
      <path d="M17 19.5 a3 3 0 1 0 2.6 1.5" stroke="#4EA064" stroke-width="1.6" stroke-linecap="round" fill="none"/>
      <polyline points="19.6,21 19.6,23 21.6,23" stroke="#4EA064" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round" fill="none"/>
    </svg>
  </div>
</div>
""";
        }

        private static string FormatJakartaTimestamp(DateTime dateTime)
        {
          return JakartaTime
            .ToJakartaOffset(dateTime)
            .ToString("dd MMM yyyy, HH:mm 'WIB'", CultureInfo.InvariantCulture);
        }

        private static EmailTemplateContent GetContentByType(OtpEmailType emailType)
        {
            return emailType switch
            {
                OtpEmailType.EmailVerification => new EmailTemplateContent(
                    "Verifikasi Email",
                    "green",
                    """
<svg width="14" height="14" viewBox="0 0 16 16" fill="none" aria-hidden="true">
  <path d="M13.3 2.7L6 10 2.7 6.7 1.3 8.1l4.7 4.7 8.7-8.7-1.4-1.4z" fill="#2E7D42"/>
</svg>
""",
                    "Masukkan kode OTP berikut untuk memverifikasi alamat email kamu. Kode ini bersifat rahasia dan hanya berlaku sekali.",
                    "Jika kamu tidak membuat permintaan ini, abaikan email ini. Akun kamu tetap aman."),
                OtpEmailType.PasswordReset => new EmailTemplateContent(
                    "Reset Password",
                    "amber",
                    """
<svg width="14" height="14" viewBox="0 0 16 16" fill="none" aria-hidden="true">
  <rect x="3" y="7" width="10" height="8" rx="1.5" fill="#B45309"/>
  <path d="M5 7V5a3 3 0 0 1 6 0v2" stroke="#B45309" stroke-width="1.5" stroke-linecap="round" fill="none"/>
</svg>
""",
                    "Gunakan kode OTP berikut untuk membuat password baru. Jangan bagikan kode ini kepada siapapun.",
                    "Jika kamu tidak membuat permintaan ini, segera ganti password dan amankan akunmu."),
                _ => throw new ArgumentOutOfRangeException(nameof(emailType), emailType, null)
            };
        }

        private enum OtpEmailType
        {
            EmailVerification,
            PasswordReset
        }

        private sealed record EmailTemplateContent(
            string SectionLabel,
            string BadgeClass,
            string BadgeIconSvg,
            string IntroText,
            string WarningText);
    }
}

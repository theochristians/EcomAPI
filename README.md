# EComAPI

Backend API e-commerce berbasis .NET 8 dengan Clean Architecture.

## Arsitektur

Solusi ini dipisah menjadi layer berikut:

- `EComAPI.Domain`: entity, value object, dan aturan bisnis.
- `EComAPI.Application`: use case (commands/queries), contract interface, validasi alur aplikasi.
- `EComAPI.Infrastructure`: implementasi teknis (EF Core, repository, caching, security).
- `EComAPI.API`: HTTP API, auth/authorization middleware, Swagger, rate limiting.
- `tests/EComAPI.Application.Tests`: unit tests application layer.
- `tests/EComAPI.API.Tests`: integration tests API layer.

## Fitur Utama

- JWT auth: register, login, refresh token, logout.
- Email verification OTP (public send/verify) dan login hanya untuk email terverifikasi.
- Forgot password berbasis OTP (`forgot-password` + `reset-password`).
- Token blacklist dengan audit DB + cache Redis/InMemory.
- Permission-based authorization.
- Modul user profile dan address.
- Modul category dan product (termasuk variant/image).
- Modul shopping: cart dan wishlist.
- Modul transaction: order, coupon, review, return, order status log, stock log.
- Rate limiting untuk endpoint auth dan email verification.

## Prasyarat

- .NET SDK 8.x
- SQL Server (Express/Developer/Azure SQL boleh)

## Menjalankan Project

1. Restore dependencies:

```powershell
dotnet restore
```

2. Update database dari migration yang sudah ada:

```powershell
dotnet ef database update --project EComAPI.Infrastructure --startup-project EComAPI.API
```

3. Run API:

```powershell
dotnet run --project EComAPI.API
```

4. Buka Swagger:

- `http://localhost:5006/swagger`
- `https://localhost:7091/swagger`

## Konfigurasi Penting

### Secret Manager / Environment Variables (Recommended)

Jangan simpan credential production di `appsettings*.json`.
Gunakan:

1. `dotnet user-secrets` untuk local development.
2. Environment variable / CI secret manager untuk staging & production.

Project API sudah diaktifkan `UserSecretsId` agar bisa langsung pakai user-secrets.

Contoh set local secret:

```powershell
dotnet user-secrets --project EComAPI.API set "ConnectionStrings:SQLServerThrifted" "Server=.;Database=ThriftedDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;"
dotnet user-secrets --project EComAPI.API set "JwtSettings:Secret" "<JWT_SECRET>"
dotnet user-secrets --project EComAPI.API set "AzureBlobStorage:Enabled" "true"
dotnet user-secrets --project EComAPI.API set "AzureBlobStorage:AccountName" "<STORAGE_ACCOUNT>"
dotnet user-secrets --project EComAPI.API set "AzureBlobStorage:ContainerName" "<CONTAINER>"
dotnet user-secrets --project EComAPI.API set "AzureBlobStorage:TenantId" "<TENANT_ID>"
dotnet user-secrets --project EComAPI.API set "AzureBlobStorage:ClientId" "<CLIENT_ID>"
dotnet user-secrets --project EComAPI.API set "AzureBlobStorage:ClientSecret" "<CLIENT_SECRET>"
```

Contoh set environment variable (PowerShell):

```powershell
$env:ConnectionStrings__SQLServerThrifted = "<CONNECTION_STRING>"
$env:JwtSettings__Secret = "<JWT_SECRET>"
$env:AzureBlobStorage__Enabled = "true"
$env:AzureBlobStorage__AccountName = "<STORAGE_ACCOUNT>"
$env:AzureBlobStorage__ContainerName = "<CONTAINER>"
$env:AzureBlobStorage__TenantId = "<TENANT_ID>"
$env:AzureBlobStorage__ClientId = "<CLIENT_ID>"
$env:AzureBlobStorage__ClientSecret = "<CLIENT_SECRET>"
```

Template key lengkap tersedia di:

- [.env.example](.env.example)

### Database

- Connection string key: `ConnectionStrings:SQLServerThrifted`

Catatan design-time EF:
`AppDbContextFactory` saat ini membaca connection string dari
`EComAPI.API/appsettings.json` (key `ConnectionStrings:SQLServerThrifted`).

### JWT

- `JwtSettings:Secret`
- `JwtSettings:Issuer`
- `JwtSettings:Audience`
- `JwtSettings:ExpiryMinutes`

### Token Blacklist

- `TokenBlacklist:FallbackMinutes`
- `TokenBlacklist:MaxMinutes`
- `TokenBlacklist:CleanupIntervalMinutes`

### Rate Limiting

Section config yang dipakai:

- `RateLimiting:RejectionMessage`
- `RateLimiting:AuthRegister:PermitLimit`
- `RateLimiting:AuthRegister:WindowMinutes`
- `RateLimiting:AuthLogin:PermitLimit`
- `RateLimiting:AuthLogin:WindowMinutes`
- `RateLimiting:EmailVerifySend:PermitLimit`
- `RateLimiting:EmailVerifySend:WindowMinutes`
- `RateLimiting:EmailVerifyCheck:PermitLimit`
- `RateLimiting:EmailVerifyCheck:WindowMinutes`
- `RateLimiting:UploadSas:PermitLimit`
- `RateLimiting:UploadSas:WindowMinutes`
- `RateLimiting:UploadConfirm:PermitLimit`
- `RateLimiting:UploadConfirm:WindowMinutes`

### Redis (Upstash REST)

Bisa pakai config file atau env var:

- `Redis:Enabled`
- `Redis:Url`
- `Redis:Token`
- `Redis:InstanceName`
- `REDIS_URL` (env)
- `REDIS_TOKEN` (env)

Jika Redis tidak aktif/credential kosong, service fallback ke in-memory cache.

### Email (Brevo SMTP)

OTP email verification akan dikirim melalui Brevo SMTP jika konfigurasi valid.
Jika tidak valid/nonaktif, service fallback ke `ConsoleEmailSender`.

- `Email:Enabled`
- `Email:SmtpHost` (default: `smtp-relay.brevo.com`)
- `Email:SmtpPort` (default: `587`)
- `Email:SmtpUsername`
- `Email:SmtpPassword`
- `Email:FromEmail`
- `Email:FromName`
- `Email:BrandName` (default: `Thrifties`)
- `Email:BrandDomain` (default: `thrifties.app`)
- `Email:LogoUrl` (opsional, URL HTTPS logo brand)
- `Email:EnableSsl`

Env var yang didukung:

- `EMAIL_ENABLED`
- `EMAIL_SMTP_HOST`
- `EMAIL_SMTP_PORT`
- `EMAIL_SMTP_USERNAME`
- `EMAIL_SMTP_PASSWORD`
- `EMAIL_FROM_EMAIL`
- `EMAIL_FROM_NAME`
- `EMAIL_BRAND_NAME`
- `EMAIL_BRAND_DOMAIN`
- `EMAIL_LOGO_URL`
- `EMAIL_ENABLE_SSL`

### Azure Blob Storage (SAS Upload)

Fitur ini dipakai untuk direct upload dari frontend ke Azure Blob (backend hanya generate SAS).

- `AzureBlobStorage:Enabled`
- `AzureBlobStorage:AccountName`
- `AzureBlobStorage:ContainerName`
- `AzureBlobStorage:TenantId`
- `AzureBlobStorage:ClientId`
- `AzureBlobStorage:ClientSecret`

Env var yang didukung:

- `AZURE_STORAGE_ACCOUNT_NAME`
- `AZURE_STORAGE_CONTAINER`
- `AZURE_TENANT_ID`
- `AZURE_CLIENT_ID`
- `AZURE_CLIENT_SECRET`

## Seeder Default

Saat startup, API menjalankan seeder role, permission, dan SYSTEM user.

## Testing

Jalankan semua test:

```powershell
dotnet test
```

Jalankan per test project:

```powershell
dotnet test tests/EComAPI.Application.Tests/EComAPI.Application.Tests.csproj
dotnet test tests/EComAPI.API.Tests/EComAPI.API.Tests.csproj
```

Dokumentasi test detail ada di [tests/README.md](tests/README.md).

## Manual Demo / Smoke Flow

Flow manual end-to-end ada di:

- [EComAPI.API/LIVE_DEMO_TESTING.md](EComAPI.API/LIVE_DEMO_TESTING.md)

Dokumen tersebut berisi urutan test manual Auth -> Catalog -> Cart/Order -> Review/Return -> Refresh/Logout.

## Timezone Policy

Aturan penggunaan waktu Jakarta vs UTC ada di:

- [TIME_POLICY.md](TIME_POLICY.md)

## Catatan Deployment

- Jangan deploy dari worktree yang masih banyak perubahan campuran.
- Simpan secret production di environment variable atau secret manager, bukan hardcoded di `appsettings`.

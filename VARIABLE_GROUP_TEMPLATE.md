# EComAPI Variable Group Template

Gunakan daftar ini untuk Azure DevOps Variable Group (atau platform sejenis).

## Required

| Variable | Value | Secret |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` | No |
| `ConnectionStrings__SQLServerThrifted` | `Server=tcp:<sql-server>.database.windows.net,1433;Initial Catalog=<db-name>;Persist Security Info=False;User ID=<db-user>;Password=<db-password>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;` | Yes |
| `JwtSettings__Secret` | `<strong-random-secret-min-32-char>` | Yes |
| `JwtSettings__Issuer` | `EComAPI` | No |
| `JwtSettings__Audience` | `EComAPIClients` | No |
| `JwtSettings__ExpiryMinutes` | `120` | No |
| `TokenBlacklist__FallbackMinutes` | `15` | No |
| `TokenBlacklist__MaxMinutes` | `120` | No |
| `TokenBlacklist__CleanupIntervalMinutes` | `60` | No |
| `Cors__AllowedOrigins__0` | `https://thrifties.app` | No |
| `Cors__AllowedOrigins__1` | `https://www.thrifties.app` | No |

## Rate Limiting (Optional Override)

Gunakan section ini jika mau override default rate limit dari `appsettings.json` via Variable Group.

| Variable | Value | Secret |
|---|---|---|
| `RateLimiting__RejectionMessage` | `Too many requests. Please try again later.` | No |
| `RateLimiting__AuthRegister__PermitLimit` | `5` | No |
| `RateLimiting__AuthRegister__WindowMinutes` | `10` | No |
| `RateLimiting__AuthLogin__PermitLimit` | `10` | No |
| `RateLimiting__AuthLogin__WindowMinutes` | `1` | No |
| `RateLimiting__EmailVerifySend__PermitLimit` | `3` | No |
| `RateLimiting__EmailVerifySend__WindowMinutes` | `10` | No |
| `RateLimiting__EmailVerifyCheck__PermitLimit` | `15` | No |
| `RateLimiting__EmailVerifyCheck__WindowMinutes` | `10` | No |
| `RateLimiting__UploadSas__PermitLimit` | `30` | No |
| `RateLimiting__UploadSas__WindowMinutes` | `5` | No |
| `RateLimiting__UploadConfirm__PermitLimit` | `60` | No |
| `RateLimiting__UploadConfirm__WindowMinutes` | `5` | No |

## Bootstrap Admin (Saat Ini Tidak Dipakai Seeder)

Variabel ini disiapkan untuk skenario bootstrap admin, tetapi kode seeder saat ini tidak membaca `BootstrapAdmin__*`.

| Variable | Value | Secret |
|---|---|---|
| `BootstrapAdmin__Enabled` | `true` | No |
| `BootstrapAdmin__FullName` | `Theo Christian Siringo Ringo` | No |
| `BootstrapAdmin__Email` | `theohristian.sir@gmail.com` | No |
| `BootstrapAdmin__Password` | `@@TenkiNoKo11` | Yes |

## Redis (Optional)

| Variable | Value | Secret |
|---|---|---|
| `Redis__Enabled` | `true` | No |
| `Redis__Url` | `https://<your-upstash-endpoint>.upstash.io` | No |
| `Redis__Token` | `<upstash-token>` | Yes |
| `Redis__InstanceName` | `ecomapi:prod:` | No |

## Email (Optional)

| Variable | Value | Secret |
|---|---|---|
| `Email__Enabled` | `true` | No |
| `Email__SmtpHost` | `smtp-relay.brevo.com` | No |
| `Email__SmtpPort` | `587` | No |
| `Email__SmtpUsername` | `<smtp-username>` | Yes |
| `Email__SmtpPassword` | `<smtp-password>` | Yes |
| `Email__FromEmail` | `no-reply@yourdomain.com` | No |
| `Email__FromName` | `Thrifted Support` | No |
| `Email__BrandName` | `Thrifties` | No |
| `Email__BrandDomain` | `thrifties.app` | No |
| `Email__LogoUrl` | `https://<your-storage>/assets/logo.png` | No |
| `Email__EnableSsl` | `true` | No |

## Azure Blob (Optional)

| Variable | Value | Secret |
|---|---|---|
| `AzureBlobStorage__Enabled` | `true` | No |
| `AzureBlobStorage__AccountName` | `<storage-account-name>` | No |
| `AzureBlobStorage__ContainerName` | `<container-name>` | No |
| `AzureBlobStorage__TenantId` | `<tenant-id-guid>` | No |
| `AzureBlobStorage__ClientId` | `<client-id-guid>` | No |
| `AzureBlobStorage__ClientSecret` | `<client-secret>` | Yes |

## Notes

- Jika variable group sudah dipakai, nilai di variable group akan override `appsettings.json`.
- Untuk production, simpan semua kredensial sebagai `Secret`.
- Hindari commit kredensial ke `appsettings*.json`.
- Gunakan format double underscore `__` untuk nested key, contoh: `ConnectionStrings__SQLServerThrifted`.
- Untuk key `Redis`, `Email`, dan `AzureBlobStorage`, aplikasi juga mendukung alias env var uppercase (`REDIS_*`, `EMAIL_*`, `AZURE_*`).

## Runbook Migrasi dan Seeding

1. Jalankan migrasi:

```powershell
dotnet ef database update --project EComAPI.Infrastructure --startup-project EComAPI.API
```

2. Jalankan API sekali untuk trigger seeding otomatis:

```powershell
dotnet run --project EComAPI.API
```

3. Setelah log seeding selesai, hentikan aplikasi.

